using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Query;
using Pawfect_API.Query.Queries;
using Pawfect_API.Repositories.Interfaces;
using System;
using Pawfect_API.Query.Interfaces;
using Pawfect_API.Services.AwsServices;

namespace Pawfect_API.BackgroundTasks.UnverifiedUserCleanupTask
{
    public class UnverifiedUserCleanupTask : BackgroundService
    {
        private readonly ILogger<UnverifiedUserCleanupTask> _logging;
        private readonly IServiceProvider _serviceProvider;
        private readonly UnverifiedUserCleanupTaskConfig _config;
        public UnverifiedUserCleanupTask
        (
            ILogger<UnverifiedUserCleanupTask> logging,
            IOptions<UnverifiedUserCleanupTaskConfig> config,
            IServiceProvider serviceProvider
        )
        {
            _logging = logging;
            _config = config.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logging.LogDebug($"Starting {nameof(UnverifiedUserCleanupTask)} ...");

            if (!_config.Enable)
            {
                _logging.LogInformation("Listener disabled. exiting");
                return;
            }

            stoppingToken.Register(() => _logging.LogInformation($"requested to stop..."));
            stoppingToken.ThrowIfCancellationRequested();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logging.LogDebug($"Going to sleep for {_config.WakeUpAfterSeconds} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(_config.WakeUpAfterSeconds), stoppingToken);
                }
                catch (TaskCanceledException ex)
                {
                    _logging.LogInformation($"Task canceled: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    _logging.LogError(ex, "Error while delaying to process notification. Continuing");
                }

                if (_config.Enable) await Process();
            }

            _logging.LogInformation($"Returning from {nameof(UnverifiedUserCleanupTask)} ...");
        }

        protected async Task Process()
        {
            using (IServiceScope serviceScope = _serviceProvider.CreateScope())
            {
                IQueryFactory queryFactory = serviceScope.ServiceProvider.GetRequiredService<IQueryFactory>();
                UserQuery userQuery = queryFactory.Query<UserQuery>();
                userQuery.Offset = 1;
                userQuery.PageSize = _config.BatchSize;
                userQuery.Fields = [nameof(Data.Entities.User.Id)];
                userQuery.IsVerified = false;
                userQuery.Roles = [UserRole.User];
                userQuery.CreatedTill = DateTime.UtcNow.AddDays(-_config.DaysPrior);

                IUserRepository userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
                IShelterRepository shelterRepository = serviceScope.ServiceProvider.GetRequiredService<IShelterRepository>();
                IFileRepository fileRepository = serviceScope.ServiceProvider.GetRequiredService<IFileRepository>();
                IAwsService awsService = serviceScope.ServiceProvider.GetRequiredService<IAwsService>();
                IMongoClient mongoClient = serviceScope.ServiceProvider.GetRequiredService<IMongoClient>();
                while (true)
                {
                    using (IClientSessionHandle session = await mongoClient.StartSessionAsync())
                    {
                        session.StartTransaction();
                        try
                        {
                            var usersToCleanup = (await userQuery.CollectAsync())?.Select(user => new { user.Id, user.ShelterId, user.ProfilePhotoId });
                            if (usersToCleanup == null || usersToCleanup.Count() == 0)
                            {
                                await session.CommitTransactionAsync();
                                break;
                            }

                            // Cleanup users
                            await userRepository.DeleteManyAsync([..usersToCleanup.Select(user => user.Id)], session);

                            // Cleanup shelters
                            if (usersToCleanup.Any(user => !String.IsNullOrEmpty(user.ShelterId)))
                            {
                                await shelterRepository.DeleteManyAsync([.. usersToCleanup.Where(user => !String.IsNullOrEmpty(user.ShelterId)).Select(user => user.ShelterId)], session);
                            }

                            // Cleanup files
                            if (usersToCleanup.Any(user => !String.IsNullOrEmpty(user.ProfilePhotoId)))
                            {
                                List<String> fileIds = [.. usersToCleanup.Where(u => !String.IsNullOrEmpty(u.ProfilePhotoId)).Select(u => u.ProfilePhotoId)];
                                List<Data.Entities.File> profilePhotos = await fileRepository.FindManyAsync(file => fileIds.Contains(file.Id), new List<String>() { nameof(Data.Entities.File.AwsKey) }, session);

                                await awsService.DeleteAsync([..profilePhotos.Select(pf => pf.AwsKey)]);
                                await fileRepository.DeleteManyAsync(fileIds, session);
                            }

                            await session.CommitTransactionAsync();
                        }
                        catch (Exception ex)
                        {
                            _logging.LogError(ex, $"Could not clean up of type: {nameof(Data.Entities.User)}");
                            await session.AbortTransactionAsync();
                            break;
                        }
                    }
                }
            }
        }
    }
}
