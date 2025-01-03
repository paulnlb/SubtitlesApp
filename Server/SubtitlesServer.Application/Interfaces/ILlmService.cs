using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Application.Interfaces;

public interface ILlmService
{
    Task<Result<string>> SendAsync(List<LlmMessageDto> chatHistory, string userPrompt);
}
