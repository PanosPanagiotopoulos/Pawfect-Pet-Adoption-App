using Pawfect_API.Query.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Pawfect_API.BackgroundTasks.RefreshTokensCleanupTask
{
    public class RefreshTokensCleanupTask : BackgroundService
    {
        private readonly ILogger<RefreshTokensCleanupTask> _logging;
        private readonly IServiceProvider _serviceProvider;
        private readonly RefreshTokensCleanupTaskConfig _config;
        public RefreshTokensCleanupTask(
            ILogger<RefreshTokensCleanupTask> logging,
            IOptions<RefreshTokensCleanupTaskConfig> config,
            IServiceProvider serviceProvider)
        {
            _logging = logging;
            _config = config.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logging.LogDebug($"Starting {nameof(RefreshTokensCleanupTask)} ...");

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

                if (_config.Enable) await this.Process();
            }

            _logging.LogInformation($"Returning from {nameof(RefreshTokensCleanupTask)} ...");
        }

        protected async Task Process()
        {
            using (IServiceScope serviceScope = _serviceProvider.CreateScope())
            {
                IRefreshTokenRepository refreshTokenRepository = serviceScope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                IMongoClient mongoClient = serviceScope.ServiceProvider.GetRequiredService<IMongoClient>();
                while (true)
                {
                    using (IClientSessionHandle session = await mongoClient.StartSessionAsync())
                    {
                        session.StartTransaction();
                        try
                        {
                            List<Pawfect_API.Data.Entities.RefreshToken> tokensToCleanup = 
                                await refreshTokenRepository.FindManyAsync(
                                    rToken => rToken.ExpiresAt >= DateTime.UtcNow, 
                                    new List<String>() { nameof(Pawfect_API.Data.Entities.RefreshToken.Id) }, 
                                    session
                            );

                            if (tokensToCleanup == null || tokensToCleanup.Count == 0)
                            {
                                await session.CommitTransactionAsync();
                                break;
                            }

                            // Delete tokens
                            await refreshTokenRepository.DeleteManyAsync([..tokensToCleanup.Select(token => token.Id)], session);

                            await session.CommitTransactionAsync();
                        }
                        catch (Exception ex)
                        {
                            _logging.LogError(ex, $"Could not clean up of type: {nameof(Pawfect_API.Data.Entities.File)}");
                            await session.AbortTransactionAsync();
                            break;
                        }
                    }
                }
            }
        }
    }
}
