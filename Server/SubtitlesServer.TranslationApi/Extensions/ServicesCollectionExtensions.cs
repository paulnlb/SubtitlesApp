using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Infrastructure.Configs;
using SubtitlesServer.Infrastructure.Mapper;
using SubtitlesServer.Infrastructure.Services;

namespace SubtitlesServer.TranslationApi.Extensions;

public static class ServicesCollectionExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<ITranslationService, LlmTranslationService>();
        services.AddSingleton<LanguageService>();
        services.AddScoped<ILlmService, OllamaLlmService>();
        services.AddAutoMapper(typeof(AutoMapperProfile));
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = new JwtConfig();
        configuration.GetSection("JwtSettings").Bind(jwtConfig);
        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = jwtConfig.Authority;
                options.TokenValidationParameters.ValidIssuer = jwtConfig.ValidIssuer;
                options.TokenValidationParameters.ValidateAudience = jwtConfig.ValidateAudience;
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