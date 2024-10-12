using Microsoft.AspNetCore.RateLimiting;
using SubtitlesServer.Application.Constants;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Application.Services;
using SubtitlesServer.Infrastructure.Configs;
using SubtitlesServer.Infrastructure.Services;
using System.Threading.RateLimiting;

namespace SubtitlesServer.WhisperApi.Extensions;

public static class ServicesCollectionExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<ITranscriptionService, TranscriptionService>();
        services.AddScoped<IWhisperService, WhisperService>();
        services.AddScoped<IWaveService, WaveService>();
        services.AddSingleton<WhisperModelService>();
    }

    public static void AddConcurrencyRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimiterConfig = new RateLimiterConfig();
        configuration.GetSection("RateLimiterSettings").Bind(rateLimiterConfig);

        services.AddRateLimiter(options =>
            options.AddConcurrencyLimiter(
                policyName: RateLimiterConstants.WhisperPolicy,
                options => {
                    options.PermitLimit = rateLimiterConfig.PermitLimit;
                    options.QueueLimit = rateLimiterConfig.QueueLimit;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                }
            )
            .OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                return new ValueTask();
            });
    }
}