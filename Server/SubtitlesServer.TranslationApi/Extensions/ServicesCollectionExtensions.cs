using SubtitlesApp.Core.Services;
using SubtitlesServer.Shared.Configs;
using SubtitlesServer.Shared.Middleware;
using SubtitlesServer.TranslationApi.Configs;
using SubtitlesServer.TranslationApi.Mapper;
using SubtitlesServer.TranslationApi.Services;
using SubtitlesServer.TranslationApi.Services.Interfaces;

namespace SubtitlesServer.TranslationApi.Extensions;

public static class ServicesCollectionExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<ITranslationService, LlmTranslationService>();
        services.AddScoped<ILlmService, OllamaLlmService>();
        services.AddScoped<CustomBearerEvents>();
        services.AddSingleton<LanguageService>();
        services.AddAutoMapper(typeof(AutoMapperProfile));
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

    public static void AddHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        var ollamaConfig = new OllamaConfig();
        configuration.GetSection("OllamaConfig").Bind(ollamaConfig);

        services.AddHttpClient<ILlmService, OllamaLlmService>(client =>
        {
            client.BaseAddress = new Uri(ollamaConfig.BaseUrl);
        });
    }
}
