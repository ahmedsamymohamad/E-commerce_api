using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce_API.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };
        if (status == 500) logger.LogError(exception, "Unhandled API exception.");
        else logger.LogWarning(exception, "API request failed with {StatusCode}.", status);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title, Detail = status == 500 ? "Please try again later." : exception.Message, Instance = context.Request.Path }, ct);
        return true;
    }
}
