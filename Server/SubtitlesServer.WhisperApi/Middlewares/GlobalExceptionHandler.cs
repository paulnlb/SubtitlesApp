using Microsoft.AspNetCore.Diagnostics;
using SubtitlesApp.Core.Result;
using System.Text.Json;

namespace SubtitlesServer.WhisperApi.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var response = new Error(ErrorCode.InternalServerError, $"An unexpected error occurred: {exception.Message}.");
        var json = JsonSerializer.Serialize(response);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(json, cancellationToken);

        logger.LogError("Error Message: {exceptionMessage}, Time of occurrence {time}", exception.Message, DateTime.UtcNow);

        return true;
    }
}
