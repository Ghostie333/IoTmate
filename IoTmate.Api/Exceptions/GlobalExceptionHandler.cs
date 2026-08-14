using IoTmate.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Api.Exceptions
{
    // Maps domain exceptions to HTTP status codes and returns a consistent ProblemDetails body
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is DomainException domainException)
            {
                httpContext.Response.StatusCode = domainException.StatusCode;

                var problemDetails = new ProblemDetails
                {
                    Status = domainException.StatusCode,
                    Title = GetTitle(domainException.StatusCode),
                    Detail = domainException.Message
                };

                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                return true;
            }

            _logger.LogError(exception, "Unhandled exception during request processing");

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            }, cancellationToken);
            return true;
        }

        private static string GetTitle(int statusCode) => statusCode switch
        {
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Error"
        };
    }
}