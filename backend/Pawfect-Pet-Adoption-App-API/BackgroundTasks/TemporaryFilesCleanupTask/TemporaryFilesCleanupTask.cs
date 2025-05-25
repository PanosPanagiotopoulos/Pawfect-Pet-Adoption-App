using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.BackgroundTasks.UnverifiedUserCleanupTask;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Query.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AwsServices;

namespace Pawfect_Pet_Adoption_App_API.BackgroundTasks.TemporaryFilesCleanupTask
{
    public class TemporaryFilesCleanupTask : BackgroundService
    {
        private readonly ILogger<TemporaryFilesCleanupTask> _logging;
        private readonly IServiceProvider _serviceProvider;
        private readonly TemporaryFilesCleanupTaskConfig _config;
        public TemporaryFilesCleanupTask(
            ILogger<TemporaryFilesCleanupTask> logging,
            IOptions<TemporaryFilesCleanupTaskConfig> config,
            IServiceProvider serviceProvider)
        {
            _logging = logging;
            _config = config.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logging.LogDebug($"Starting {nameof(TemporaryFilesCleanupTask)} ...");

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

            _logging.LogInformation($"Returning from {nameof(TemporaryFilesCleanupTask)} ...");
        }

        protected async Task Process()
        {
            using (IServiceScope serviceScope = _serviceProvider.CreateScope())
            {
                QueryFactory queryFactory = serviceScope.ServiceProvider.GetRequiredService<QueryFactory>();
                FileQuery fileQuery = queryFactory.Query<FileQuery>();
                fileQuery.Offset = 1;
                fileQuery.PageSize = _config.BatchSize;
                fileQuery.Fields = [nameof(Data.Entities.File.Id), nameof(Data.Entities.File.OwnerId)];
                fileQuery.FileSaveStatuses = [FileSaveStatus.Temporary];
                fileQuery.CreatedTill = DateTime.UtcNow.AddMinutes(-_config.MinutesPrior);

                IFileRepository fileRepository = _serviceProvider.GetRequiredService<IFileRepository>();
                IAwsService awsService = _serviceProvider.GetRequiredService<IAwsService>();
                IMongoClient mongoClient = serviceScope.ServiceProvider.GetRequiredService<IMongoClient>();
                while (true)
                {
                    using (IClientSessionHandle session = await mongoClient.StartSessionAsync())
                    {
                        session.StartTransaction();
                        try
                        {
                            List<Data.Entities.File> filesToCleanup = await fileQuery.CollectAsync();
                            if (filesToCleanup == null || filesToCleanup.Count == 0)
                            {
                                await session.CommitTransactionAsync();
                                break;
                            }

                            // Get the aws keys of the files
                            List<String> keys = [..filesToCleanup.Select(file => { return awsService.ConstructAwsKey(file.Id, file.OwnerId); })];
                            // Delete the files from aws
                            Dictionary<String, Boolean> results = await awsService.DeleteAsync(keys);

                            List<String> failedIds = [.. results.Where(r => !r.Value).Select(r => r.Key.Split('-')[0])];
                            List<String> succeededIds = [..filesToCleanup.Where(f => !failedIds.Contains(f.Id)).Select(f => f.Id)];
                            if (failedIds.Count != 0)
                                _logging.LogError("Not all objects where deleted from AWS. Removing them from file deleting pipeline");

                            // Delete corresponding files from database
                            await fileRepository.DeleteAsync(filesToCleanup, session);
                            
                            await session.CommitTransactionAsync();
                        }
                        catch (Exception ex)
                        {
                            _logging.LogError(ex, $"Could not clean up of type: {nameof(Data.Entities.File)}");
                            await session.AbortTransactionAsync();
                            break;
                        }
                    }
                }
            }
        }
    }
}
