using SubtitlesApp.Core.Result;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Interfaces;

public interface ILlmService
{
    Task<Result<string>> SendChatAsync(List<LlmMessageDto> chatHistory, string userPrompt, object? responseFormat = null);

    AsyncEnumerableResult<string> StreamChatAsync(
        List<LlmMessageDto> chatHistory,
        string userPrompt,
        object? responseFormat = null
    );
}
