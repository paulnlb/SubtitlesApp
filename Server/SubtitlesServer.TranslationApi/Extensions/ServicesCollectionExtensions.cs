using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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
        services.AddScoped<ILlmService, OpenAILlmService>();
        services.AddAutoMapper(typeof(AutoMapperProfile));
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        services.AddSingleton<IChatCompletionService>(sp =>
        {
            OpenAIConfig openAIConfig = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;
            return new OpenAIChatCompletionService(openAIConfig.ModelName, openAIConfig.ApiKey);
        });
        services.AddKeyedTransient<Kernel>("OpenAIKernel");
    }

    public static void AddHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        var ollamaConfig = new OllamaConfig();
        configuration.GetSection("OllamaConfig").Bind(ollamaConfig);

        //services.AddHttpClient<ILlmService, OllamaLlmService>(client =>
        //{
        //    client.BaseAddress = new Uri(ollamaConfig.BaseUrl);
        //});
    }
}
