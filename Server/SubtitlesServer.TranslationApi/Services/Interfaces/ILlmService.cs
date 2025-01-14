using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.TranslationApi.Services.Interfaces;

public interface ILlmService
{
    Task<Result<string>> SendAsync(List<LlmMessageDto> chatHistory, string userPrompt);

    IAsyncEnumerable<string> StreamAsync(List<LlmMessageDto> chatHistory, string userPrompt);
}
