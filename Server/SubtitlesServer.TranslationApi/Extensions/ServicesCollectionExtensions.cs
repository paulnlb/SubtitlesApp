using FluentValidation;
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
        services.AddAutoMapper(typeof(AutoMapperProfile));
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
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
