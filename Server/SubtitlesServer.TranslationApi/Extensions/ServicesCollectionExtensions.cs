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
    public static void AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITranslationService, LlmTranslationService>();
        services.AddAutoMapper(typeof(AutoMapperProfile));
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        var llmTranslationConfig = new LlmTranslationConfig();
        configuration.GetSection("LlmTranslationConfig").Bind(llmTranslationConfig);

        switch (llmTranslationConfig.LlmProvider)
        {
            case LlmProvider.Ollama:
                AddOllamaService(services, configuration);
                break;
            case LlmProvider.OpenAi:
                AddOpenAiService(services);
                break;
        }
    }

    public static void AddOpenAiService(this IServiceCollection services)
    {
        services.AddScoped<ILlmService, OpenAILlmService>();

        services.AddSingleton<IChatCompletionService>(sp =>
        {
            OpenAIConfig openAIConfig = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;
            return new OpenAIChatCompletionService(openAIConfig.ModelName, openAIConfig.ApiKey);
        });
        services.AddKeyedTransient<Kernel>("OpenAIKernel");
    }

    public static void AddOllamaService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILlmService, OllamaLlmService>();

        var ollamaConfig = new OllamaConfig();
        configuration.GetSection("OllamaConfig").Bind(ollamaConfig);

        services.AddHttpClient<ILlmService, OllamaLlmService>(client =>
        {
            client.BaseAddress = new Uri(ollamaConfig.BaseUrl);
        });
    }
}
