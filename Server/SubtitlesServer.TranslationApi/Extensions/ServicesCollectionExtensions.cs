using SubtitlesApp.Core.Services;
using SubtitlesServer.Shared.Middleware;
using SubtitlesServer.TranslationApi.Configs;
using SubtitlesServer.TranslationApi.Interfaces;
using SubtitlesServer.TranslationApi.Mapper;
using SubtitlesServer.TranslationApi.Services;

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
