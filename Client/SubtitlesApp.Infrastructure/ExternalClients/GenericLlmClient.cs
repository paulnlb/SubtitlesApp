using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Infrastructure.ExternalClients;

public class GenericLlmClient(ILlmSettings settings, IServiceProvider serviceProvider) : ILlmClient
{
    public Task<Result<string>> SendChatAsync(List<LlmMessageDto> chatHistory, string userPrompt, JsonNode responseFormat)
    {
        return GetSpecificClient().SendChatAsync(chatHistory, userPrompt, responseFormat);
    }

    public Task<Result<T>> SendChatAsync<T>(List<LlmMessageDto> chatHistory, string userPrompt)
    {
        var client = serviceProvider.GetKeyedService<ILlmClient>(settings.LlmProvider);
        return GetSpecificClient().SendChatAsync<T>(chatHistory, userPrompt);
    }

    private ILlmClient GetSpecificClient()
    {
        return serviceProvider.GetKeyedService<ILlmClient>(settings.LlmProvider)
            ?? throw new InvalidOperationException($"No ILlmClient registered for provider {settings.LlmProvider}");
    }
}
