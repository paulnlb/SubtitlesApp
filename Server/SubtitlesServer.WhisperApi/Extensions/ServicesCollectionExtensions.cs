using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Shared.Configs;
using SubtitlesServer.Shared.Middleware;
using SubtitlesServer.WhisperApi.Configs;
using SubtitlesServer.WhisperApi.Services;
using SubtitlesServer.WhisperApi.Services.Interfaces;

namespace SubtitlesServer.WhisperApi.Extensions;

public static class ServicesCollectionExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<ITranscriptionService, WhisperService>();
        services.AddScoped<IWaveService, WaveService>();
        services.AddScoped<CustomBearerEvents>();
        services.AddSingleton<WhisperModelService>();
        services.AddSingleton<LanguageService>();
        services.AddSingleton<CatalystModelService>();
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

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = new JwtConfig();
        configuration.GetSection("JwtSettings").Bind(jwtConfig);
        services
            .AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = jwtConfig.Authority;
                options.TokenValidationParameters.ValidIssuer = jwtConfig.ValidIssuer;
                options.TokenValidationParameters.ValidateAudience = jwtConfig.ValidateAudience;
                options.EventsType = typeof(CustomBearerEvents);
            });
    }
}
