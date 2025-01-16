using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Shared.Middleware;
using SubtitlesServer.WhisperApi.Configs;
using SubtitlesServer.WhisperApi.Interfaces;
using SubtitlesServer.WhisperApi.Mapper;
using SubtitlesServer.WhisperApi.Services;
using SubtitlesServer.WhisperApi.Services.ModelProviders;

namespace SubtitlesServer.WhisperApi.Extensions;

public static class ServicesCollectionExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<ISpeechToTextService, WhisperService>();
        services.AddScoped<IAudioService, WaveAudioService>();
        services.AddScoped<ITranscriptionService, TranscriptionService>();
        services.AddScoped<INlpService, CatalystNlpService>();
        services.AddScoped<CustomBearerEvents>();
        services.AddSingleton<WhisperModelProvider>();
        services.AddSingleton<LanguageService>();
        services.AddSingleton<CatalystModelProvider>();
        services.AddAutoMapper(typeof(AutoMapperProfile));
    }

    public static void AddConcurrencyRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimiterConfig = new RateLimiterConfig();
        configuration.GetSection("RateLimiterSettings").Bind(rateLimiterConfig);

        services.AddRateLimiter(options =>
            options
                .AddConcurrencyLimiter(
                    policyName: RateLimiterConstants.WhisperPolicy,
                    options =>
                    {
                        options.PermitLimit = rateLimiterConfig.PermitLimit;
                        options.QueueLimit = rateLimiterConfig.QueueLimit;
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    }
                )
                .OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                return new ValueTask();
            }
        );
    }
}
