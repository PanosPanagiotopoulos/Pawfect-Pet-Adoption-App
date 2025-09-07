using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;

namespace Pawfect_Messenger.Transactions
{
    public class MongoTransactionFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<MongoTransactionFilter> _logger;
        public int Order { get; set; } = 0;

        public MongoTransactionFilter(IMongoClient mongoClient, ILogger<MongoTransactionFilter> logger)
        {
            _mongoClient = mongoClient;
            _logger = logger;
        }

        protected virtual Task OnPreRollback()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnPostRollback()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnPreCommit()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnPostCommit()
        {
            return Task.CompletedTask;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Start a client session
            using IClientSessionHandle session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                _logger.LogTrace("Started MongoDB transaction for action {ActionName}", context.ActionDescriptor.DisplayName);

                // Store the session in HttpContext.Items for use in repositories
                context.HttpContext.Items["MongoSession"] = session;

                // Execute the action
                ActionExecutedContext resultContext = await next();

                if (resultContext.Exception == null)
                {
                    // Action succeeded, commit the transaction
                    await OnPreCommit();
                    _logger.LogTrace("Committing MongoDB transaction for action {ActionName}", context.ActionDescriptor.DisplayName);
                    await session.CommitTransactionAsync();
                    await OnPostCommit();
                }
                else
                {
                    // Action failed, rollback the transaction
                    await OnPreRollback();
                    _logger.LogDebug("Rolling back MongoDB transaction for action {ActionName} due to exception", context.ActionDescriptor.DisplayName);
                    await session.AbortTransactionAsync();
                    await OnPostRollback();
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors, rollback the transaction
                await OnPreRollback();
                _logger.LogDebug(ex, "Rolling back MongoDB transaction for action {ActionName} due to unexpected error", context.ActionDescriptor.DisplayName);
                await session.AbortTransactionAsync();
                await OnPostRollback();
                throw;
            }
            finally
            {
                // Ensure the session is removed from HttpContext.Items
                context.HttpContext.Items.Remove("MongoSession");
            }
        }
    }
}
