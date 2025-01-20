using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Shared.Configs;
using SubtitlesServer.Shared.FluentValidation;
using SubtitlesServer.Shared.Middleware;

namespace SubtitlesServer.Shared.Extensions;

public static class ServicesCollectionExtensions
{
    public static void AddAutoFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(configuration =>
        {
            configuration.EnableFormBindingSourceAutomaticValidation = true;
            configuration.DisableBuiltInModelValidation = true;
            configuration.OverrideDefaultResultFactoryWith<CustomResultFactory>();
        });
        ValidatorOptions.Global.LanguageManager.Enabled = false;
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

    public static void AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CustomBearerEvents>();
        services.AddSingleton<LanguageService>();

        services.AddAutoFluentValidation();

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddJwtAuthentication(configuration);
        services.AddAuthorization();
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
