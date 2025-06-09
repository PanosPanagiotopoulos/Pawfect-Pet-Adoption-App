using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using System;

namespace Pawfect_Pet_Adoption_App_API.BackgroundTasks.UnverifiedUserCleanupTask
{
    public class UnverifiedUserCleanupTask : BackgroundService
    {
        private readonly ILogger<UnverifiedUserCleanupTask> _logging;
        private readonly IServiceProvider _serviceProvider;
        private readonly UnverifiedUserCleanupTaskConfig _config;
        public UnverifiedUserCleanupTask(
            ILogger<UnverifiedUserCleanupTask> logging,
            IOptions<UnverifiedUserCleanupTaskConfig> config,
            IServiceProvider serviceProvider)
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
                QueryFactory queryFactory = serviceScope.ServiceProvider.GetRequiredService<QueryFactory>();
                UserQuery userQuery = queryFactory.Query<UserQuery>();
                userQuery.Offset = 1;
                userQuery.PageSize = _config.BatchSize;
                userQuery.Fields = [nameof(Data.Entities.User.Id)];
                userQuery.IsVerified = false;
                userQuery.Roles = [UserRole.User];
                userQuery.CreatedTill = DateTime.UtcNow.AddDays(-_config.DaysPrior);

                IUserRepository userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
                IMongoClient mongoClient = serviceScope.ServiceProvider.GetRequiredService<IMongoClient>();
                while (true)
                {
                    using (IClientSessionHandle session = await mongoClient.StartSessionAsync())
                    {
                        session.StartTransaction();
                        try
                        {
                            List<String> usersToCleanup = [..(await userQuery.CollectAsync())?.Select(user => user.Id)];
                            if (usersToCleanup == null || usersToCleanup.Count == 0)
                            {
                                await session.CommitTransactionAsync();
                                break;
                            }

                            await userRepository.DeleteAsync(usersToCleanup, session);
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
