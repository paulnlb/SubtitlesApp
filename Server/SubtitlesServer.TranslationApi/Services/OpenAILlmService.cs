using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
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
        JsonNode? responseFormat = null
    )
    {
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var semanticKernelChatHistory = new ChatHistory();
        var chatMessagesContents = chatHistory.Select(chh => new Microsoft.SemanticKernel.ChatMessageContent(
            new AuthorRole(chh.Role),
            chh.Content
        ));

        semanticKernelChatHistory.AddRange(chatMessagesContents);
        semanticKernelChatHistory.AddUserMessage(userPrompt);

        var executionSettings = new OpenAIPromptExecutionSettings();

        if (responseFormat != null)
        {
            SetResponseFormat(executionSettings, responseFormat);
        }

        try
        {
            var result = await chatCompletionService.GetChatMessageContentAsync(
                semanticKernelChatHistory,
                executionSettings,
                kernel
            );
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
        JsonNode? responseFormat = null
    )
    {
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var semanticKernelChatHistory = new ChatHistory();
        var chatMessagesContents = chatHistory.Select(chh => new Microsoft.SemanticKernel.ChatMessageContent(
            new AuthorRole(chh.Role),
            chh.Content
        ));

        semanticKernelChatHistory.AddRange(chatMessagesContents);
        semanticKernelChatHistory.AddUserMessage(userPrompt);

        var executionSettings = new OpenAIPromptExecutionSettings();

        if (responseFormat != null)
        {
            SetResponseFormat(executionSettings, responseFormat);
        }

        try
        {
            var resultParts = chatCompletionService.GetStreamingChatMessageContentsAsync(
                semanticKernelChatHistory,
                executionSettings,
                kernel
            );

            return AsyncEnumerableResult<string>.Success(resultParts.Select(content => content.Content ?? string.Empty));
        }
        catch (Exception ex)
        {
            logger.LogError("OpenAI API returned an error: {error}", ex.Message);
            var error = new Error(ErrorCode.BadGateway, ex.Message);
            return AsyncEnumerableResult<string>.Failure(error);
        }
    }

    private static void SetResponseFormat(OpenAIPromptExecutionSettings executionSettings, JsonNode responseFormat)
    {
        var responseFormatStr = responseFormat.ToJsonString();

        var chatResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "translation",
            jsonSchema: BinaryData.FromString(responseFormatStr),
            jsonSchemaIsStrict: true
        );

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        executionSettings.ResponseFormat = chatResponseFormat;
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }
}
