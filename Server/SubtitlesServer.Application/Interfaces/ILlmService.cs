using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.Application.Interfaces;

public interface ILlmService
{
    Task<string> SendAsync(List<LlmMessageDto> chatHistory, string userPrompt);
}
