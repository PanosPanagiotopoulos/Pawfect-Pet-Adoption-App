using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pawfect_Notifications.Query;
using Pawfect_Notifications.Repositories.Interfaces;
using Pawfect_Notifications.Services.Convention;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Data.Entities.Types.Notifications;
using Pawfect_Notifications.Query.Queries;
using MongoDB.Driver;
using Pawfect_Notifications.Services.MongoServices;
using Pawfect_Notifications.Data.Entities;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pawfect_Notifications.Services.NotificationServices.Senders;

namespace Pawfect_Notifications.BackgroundTasks.NotificationProcessor
{
    public class NotificationProcessorTask : BackgroundService
    {
        private readonly ILogger<NotificationProcessorTask> _logger;
        private readonly NotificationProcessorConfig _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new Random();

        public NotificationProcessorTask(
            ILogger<NotificationProcessorTask> logger,
            IOptions<NotificationProcessorConfig> config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification processor starting...");

            stoppingToken.Register(() => _logger.LogInformation("Notification processor stop requested..."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Going to sleep for {IntervalSeconds} seconds...", _config.IntervalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(_config.IntervalSeconds), stoppingToken);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogInformation("Task canceled: {Message}", ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while delaying notification processing. Continuing...");
                }

                if (_config.Enable)
                {
                    await this.Proccess();
                }
            }

            _logger.LogInformation("Notification processor stopping...");
        }

        private async Task Proccess()
        {
            try
            {
                _logger.LogDebug("Processing pending notifications...");

                DateTime? lastCandidateCreationTimestamp = null;
                Int32 processedCount = 0;

                while (processedCount < _config.BatchSize)
                {
                    using (IServiceScope serviceScope = _serviceProvider.CreateScope())
                    {
                        IMongoClient mongoClient = serviceScope.ServiceProvider.GetRequiredService<IMongoClient>();
                        using (IClientSessionHandle session = await mongoClient.StartSessionAsync())
                        {
                            NotificationCandidate candidate = await this.GetCandidateToProcess(lastCandidateCreationTimestamp, serviceScope, session);
                            if (candidate == null) break;

                            lastCandidateCreationTimestamp = candidate.CreatedAt;
                            processedCount++;

                            _logger.LogDebug("Processing notification: {NotificationId}", candidate.Id);

                            Boolean shouldOmit = await this.ShouldOmitNotification(candidate, serviceScope, session);
                            if (shouldOmit)
                            {
                                _logger.LogDebug("Omitting notification {NotificationId} - too old to process", candidate.Id);
                                continue;
                            }

                            Boolean shouldWait = await this.ShouldWaitForRetry(candidate, serviceScope, session);
                            if (shouldWait)
                            {
                                _logger.LogDebug("Notification {NotificationId} not ready for retry yet", candidate.Id);
                                continue;
                            }

                            Boolean success = await this.SendNotification(candidate.Id, serviceScope, session);
                            _logger.LogDebug("Notification {NotificationId} processing result: {Success}", candidate.Id, success);
                        }

                        if (processedCount > 0)
                        {
                            _logger.LogInformation("Processed {ProcessedCount} notifications in this cycle", processedCount);
                        }
                    }
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problem processing notifications. Breaking for next interval");
            }
        }

        private async Task<NotificationCandidate> GetCandidateToProcess(DateTime? lastCandidateCreationTimestamp, IServiceScope serviceScope, IClientSessionHandle session)
        {
            IQueryFactory queryFactory = serviceScope.ServiceProvider.GetRequiredService<IQueryFactory>();
            try
            {
                NotificationQuery query = queryFactory.Query<NotificationQuery>();

                // Build the filter for pending or error notifications
                query.NotificationStatuses = new List<NotificationStatus>
                {
                    NotificationStatus.Pending,
                    NotificationStatus.Error
                };

                if (lastCandidateCreationTimestamp.HasValue)
                {
                    query.CreateFrom = lastCandidateCreationTimestamp.Value;
                }

                // Order by creation time to ensure FIFO processing
                query.SortBy = [nameof(Pawfect_Notifications.Models.Notification.Notification.CreatedAt)];
                query.SortDescending = false;
                query.Offset = 0;
                query.PageSize = 1;

                Notification notification = (await query.CollectAsync()).FirstOrDefault();

                if (notification == null) return null;

                // Check if this notification type should be processed
                if (!_config.DefaultOptions.EnableProcessing) return null;

                // Atomically update status to Processing to prevent concurrent processing
                INotificationRepository notificationRepo = serviceScope.ServiceProvider.GetRequiredService<INotificationRepository>();

                notification.Status = NotificationStatus.Processing;
                notification.ProcessedAt = DateTime.UtcNow;

                String result = await notificationRepo.UpdateAsync(notification, session);

                if (String.IsNullOrEmpty(result))
                {
                    // Another process already picked up this notification
                    return null;
                }

                return new NotificationCandidate
                {
                    Id = notification.Id,
                    PreviousStatus = notification.Status,
                    CreatedAt = notification.CreatedAt,
                    Type = notification.Type,
                    RetryCount = notification.RetryCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidate notification to process");
                return null;
            }
        }

        private async Task<Boolean> ShouldOmitNotification(NotificationCandidate candidate, IServiceScope serviceScope, IClientSessionHandle session)
        {
            try
            {
                TimeSpan age = DateTime.UtcNow - candidate.CreatedAt;
                TimeSpan omitThreshold = TimeSpan.FromSeconds(_config.DefaultOptions.TooOldToProcessSeconds);

                if (age < omitThreshold) return false;

                // Mark as omitted
                INotificationRepository notificationRepo = serviceScope.ServiceProvider.GetRequiredService<INotificationRepository>();

                Notification notification = await notificationRepo.FindAsync(notification => notification.Id == candidate.Id, session);

                notification.Status = NotificationStatus.Failed;
                notification.ProcessedAt = DateTime.UtcNow;

                await notificationRepo.UpdateAsync(notification, session);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if notification {NotificationId} should be omitted", candidate.Id);
                return true; // Omit on error to prevent infinite loops
            }
        }

        private async Task<Boolean> ShouldWaitForRetry(NotificationCandidate candidate, IServiceScope serviceScope, IClientSessionHandle session)
        {
            try
            {
                if (candidate.RetryCount == 0) return false; // First attempt, no delay

                // Calculate retry delay with exponential backoff
                Int32 baseDelay = candidate.RetryCount * _config.DefaultOptions.RetryDelayStepSeconds;
                Int32 jitter = _random.Next(0, Math.Min(baseDelay / 2, 300)); // Add up to 5 min jitter
                Int32 totalDelay = Math.Min(baseDelay + jitter, _config.DefaultOptions.MaxRetryDelaySeconds);

                DateTime retryAt = candidate.CreatedAt.AddSeconds(totalDelay);
                Boolean shouldWait = retryAt > DateTime.UtcNow;

                if (shouldWait)
                {
                    // Reset status back to previous state for retry later
                    INotificationRepository notificationRepo = serviceScope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    Notification notification = await notificationRepo.FindAsync(notification => notification.Id == candidate.Id, session);

                    notification.Status = candidate.PreviousStatus;
                    notification.ProcessedAt = null;

                    await notificationRepo.UpdateAsync(notification, session);
                }

                return shouldWait;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking retry delay for notification {NotificationId}", candidate.Id);
                return false; // Process immediately on error
            }
        }

        private async Task<Boolean> SendNotification(String notificationId, IServiceScope serviceScope, IClientSessionHandle session)
        {
            try
            {
                INotificationRepository notificationRepo = serviceScope.ServiceProvider.GetRequiredService<INotificationRepository>();
                NotificationSenderFactory notificationSenderFactory = serviceScope.ServiceProvider.GetRequiredService<NotificationSenderFactory>();

                Notification notification = await notificationRepo.FindAsync(notification => notification.Id == notificationId, session);

                if (notification == null)
                {
                    _logger.LogWarning("Notification {NotificationId} not found during processing", notificationId);
                    return false;
                }

                INotificationSender sender = notificationSenderFactory.SenderSafe(notification.Type);
                if (sender == null)
                {
                    await UpdateNotificationStatus(notificationId, false, 0, serviceScope, session);
                    return false;
                }

                Boolean success = await sender.SendAsync(notification, serviceScope, session);
                
                // Update notification status based on processing result
                await this.UpdateNotificationStatus(notificationId, success, notification.RetryCount, serviceScope, session);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification {NotificationId}", notificationId);
                await UpdateNotificationStatus(notificationId, false, 0, serviceScope, session);
                return false;
            }
        }

        private async Task UpdateNotificationStatus(String notificationId, Boolean success, Int32 currentRetryCount, IServiceScope serviceScope, IClientSessionHandle session)
        {
            try
            {
                INotificationRepository notificationRepo = serviceScope.ServiceProvider.GetRequiredService<INotificationRepository>();

                Notification notification = await notificationRepo.FindAsync(notification => notification.Id == notificationId, session);

                if (success)
                {
                    notification.Status = NotificationStatus.Completed;
                    notification.ProcessedAt = DateTime.UtcNow;
                }
                else
                {
                    if (notification != null && currentRetryCount < notification.MaxRetries)
                    {
                        // Increment retry count and set to error status for retry
                        notification.Status = NotificationStatus.Error;
                        notification.RetryCount++;
                        notification.ProcessedAt = null;

                    }
                    else
                    {
                        // Max retries reached, mark as failed
                        notification.Status = NotificationStatus.Error;
                        notification.ProcessedAt = DateTime.UtcNow;
                    }
                }

                await notificationRepo.UpdateAsync(notification, session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification {NotificationId} status", notificationId);
            }
        }
    }

    public class NotificationCandidate
    {
        public String Id { get; set; }
        public NotificationStatus PreviousStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationType Type { get; set; }
        public Int32 RetryCount { get; set; }
    }
}
