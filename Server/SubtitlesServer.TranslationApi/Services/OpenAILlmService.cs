using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SubtitlesApp.Core.Result;
using SubtitlesServer.TranslationApi.Interfaces;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Services;

public class OpenAILlmService([FromKeyedServices("OpenAIKernel")] Kernel kernel, ILogger<OpenAILlmService> logger)
    : ILlmService
{
    public async Task<Result<string>> SendChatAsync(
        List<LlmMessageDto> chatHistory,
        string userPrompt,
        object? responseFormat = null
    )
    {
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var semanticKernelChatHistory = new ChatHistory();
        var chatMessagesContents = chatHistory.Select(chh => new ChatMessageContent(new AuthorRole(chh.Role), chh.Content));

        semanticKernelChatHistory.AddRange(chatMessagesContents);
        semanticKernelChatHistory.AddUserMessage(userPrompt);

        try
        {
            var result = await chatCompletionService.GetChatMessageContentAsync(semanticKernelChatHistory, kernel: kernel);
            return Result<string>.Success(result.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError("OpenAI API returned an error: {error}", ex.Message);
            var error = new Error(ErrorCode.BadGateway, ex.Message);
            return Result<string>.Failure(error);
        }
    }

    public AsyncEnumerableResult<string> StreamChatAsync(
        List<LlmMessageDto> chatHistory,
        string userPrompt,
        object? responseFormat = null
    )
    {
        throw new NotImplementedException();
    }
}
