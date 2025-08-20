using MongoDB.Driver;
using Newtonsoft.Json;
using Pawfect_Notifications.DevTools;
using Pawfect_Notifications.Exceptions;
using Pawfect_Notifications.Middleware;
using System.Net;

namespace Pawfect_Notifications.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware
        (
            RequestDelegate next, 
            ILogger<ErrorHandlingMiddleware> logger, 
            IWebHostEnvironment env
        )
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Log exception details including request info
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            (HttpStatusCode statusCode, String message) = this.GetStatusCodeAndMessage(ex);

            // For server errors (5xx), provide detailed info in development, generic in production
            if (statusCode >= HttpStatusCode.InternalServerError && _env.IsDevelopment())
            {
                message = $"{ex.GetType().Name}: {ex.Message}\nStack Trace: {ex.StackTrace}";
            }

            // Check if response has started to avoid modifying it
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started, cannot send error response.");
                return;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new { error = message };
            await context.Response.WriteAsync(JsonHelper.SerializeObjectFormatted(response));
        }

        private (HttpStatusCode, String) GetStatusCodeAndMessage(Exception ex) => ex switch
        {
            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound,
                 $"Resource not found: {notFoundEx.Message} (EntityId: {notFoundEx.EntityId ?? "N/A"}, Type: {notFoundEx.EntityType?.Name ?? "N/A"})"),
            ForbiddenException forbiddenEx =>
                (HttpStatusCode.Forbidden,
                 $"Access denied: {forbiddenEx.Message} (Permissions: {String.Join(", ", forbiddenEx.Permissions ?? Array.Empty<String>())})"),
            UnAuthenticatedException unAuthEx =>
                (HttpStatusCode.Unauthorized,
                 $"Authentication required: {unAuthEx.Message}"),
            ArgumentNullException argNullEx =>
                (HttpStatusCode.BadRequest,
                 $"Invalid request: Argument '{argNullEx.ParamName}' cannot be null"),
            ArgumentException argEx =>
                (HttpStatusCode.BadRequest,
                 $"Invalid request: {argEx.Message}"),
            MongoException mongoEx =>
                (HttpStatusCode.InternalServerError,
                 "Internal server error"),
            InvalidOperationException invOpEx =>
                (HttpStatusCode.BadRequest,
                 $"Invalid operation requested for reasson : ${invOpEx.Message}"),
            _ => (HttpStatusCode.InternalServerError,
                  "Internal server error")
        };
    }

    /// <summary>
    /// Extension method to add the ErrorHandlingMiddleware to the application pipeline.
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
