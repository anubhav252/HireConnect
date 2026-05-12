using System.Net;
using System.Text.Json;

namespace HireConnect.Job.Middleware
{
    /// <summary>
    /// Global middleware that catches unhandled exceptions and returns
    /// structured JSON error responses with appropriate HTTP status codes.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,
                                        ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message, errors) = exception switch
            {
                NotFoundException nfe =>
                    (HttpStatusCode.NotFound, nfe.Message, (IEnumerable<string>?)null),

                JobValidationException ve =>
                    (HttpStatusCode.BadRequest, "Validation failed.", ve.Errors),

                ForbiddenException fe =>
                    (HttpStatusCode.Forbidden, fe.Message, null),

                ElasticsearchException ee =>
                    (HttpStatusCode.ServiceUnavailable, "Search service error: " + ee.Message, null),

                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, "Unauthorized.", null),

                _ =>
                    (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                statusCode = (int)statusCode,
                message,
                errors,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return context.Response.WriteAsync(json);
        }
    }

    /// <summary>Extension method to cleanly register the middleware in Program.cs.</summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
