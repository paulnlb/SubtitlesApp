using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Interfaces;

public interface ILlmService
{
    Task<Result<string>> SendAsync(List<LlmMessageDto> chatHistory, string userPrompt);

    AsyncEnumerableResult<string> StreamAsync(List<LlmMessageDto> chatHistory, string userPrompt);
}
