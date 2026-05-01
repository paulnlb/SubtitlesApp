using System.Text.Json.Nodes;
using SubtitlesServer.Shared.Result;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Interfaces;

public interface ILlmService
{
    Task<Result<string>> SendChatAsync(List<LlmMessageDto> chatHistory, string userPrompt, JsonNode? responseFormat = null);

    AsyncEnumerableResult<string> StreamChatAsync(
        List<LlmMessageDto> chatHistory,
        string userPrompt,
        JsonNode? responseFormat = null
    );
}
