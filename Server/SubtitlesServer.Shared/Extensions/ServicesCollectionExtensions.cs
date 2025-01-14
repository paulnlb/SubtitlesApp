using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubtitlesServer.Shared.Configs;
using SubtitlesServer.Shared.Middleware;

namespace SubtitlesServer.Shared.Extensions;

public static class ServicesCollectionExtensions
{
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

    public static void AddAuthorizationToPipeline(this ControllerActionEndpointConventionBuilder builder, ILogger logger)
    {
        if (Environment.GetEnvironmentVariable("ALLOW_ANONYMOUS") == "true")
        {
            logger.LogInformation("ALLOW_ANONYMOUS environment variable set to true, auth disabled");
        }
        else
        {
            builder.RequireAuthorization();
        }
    }
}
