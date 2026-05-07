using System.Text.Json.Nodes;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using SubtitlesServer.Shared.Result;
using SubtitlesServer.TranslationApi.Helpers;
using SubtitlesServer.TranslationApi.Interfaces;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Services;

public class OpenAILlmService(IChatCompletionService chatCompletionService, ILogger<OpenAILlmService> logger) : ILlmService
{
    public async Task<Result<string>> SendChatAsync(
        List<LlmMessageDto> chatHistory,
        string userPrompt,
        JsonNode? responseFormat = null
    )
    {
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
            SetupStructuredOutput(executionSettings, responseFormat);
        }

        try
        {
            var result = await chatCompletionService.GetChatMessageContentAsync(
                semanticKernelChatHistory,
                executionSettings
            );
            var resultContent = result.Content ?? string.Empty;

            // The reason for unwrapping is described in comments inside the SetupStructuredOutput method
            if (responseFormat != null && JsonHelper.IsJsonSchemaTypeOf(responseFormat, "array"))
            {
                resultContent = JsonHelper.UnwrapJsonArrayFromRootObject(resultContent);
            }

            return Result<string>.Success(resultContent);
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
            SetupStructuredOutput(executionSettings, responseFormat);
        }

        try
        {
            var resultParts = chatCompletionService.GetStreamingChatMessageContentsAsync(
                semanticKernelChatHistory,
                executionSettings
            );

            var resultsContent = resultParts.Select(content => content.Content ?? string.Empty);

            // The reason for unwrapping is described in comments inside the SetupStructuredOutput method
            if (responseFormat != null && JsonHelper.IsJsonSchemaTypeOf(responseFormat, "array"))
            {
                resultsContent = JsonHelper.UnwrapJsonArrayFromRootObjectAsync(resultsContent);
            }

            return AsyncEnumerableResult<string>.Success(resultsContent);
        }
        catch (Exception ex)
        {
            logger.LogError("OpenAI API returned an error: {error}", ex.Message);
            var error = new Error(ErrorCode.BadGateway, ex.Message);
            return AsyncEnumerableResult<string>.Failure(error);
        }
    }

    private static void SetupStructuredOutput(OpenAIPromptExecutionSettings executionSettings, JsonNode responseFormat)
    {
        // OpenAI API is not currently compatible with root-level JSON array schema for structured outputs.
        // Ref: https://community.openai.com/t/structured-outputs-with-arrays/957869
        // The workaround here is to wrap the array into an object
        // and then unwrap it back when parsing the OpenAi response

        if (JsonHelper.IsJsonSchemaTypeOf(responseFormat, "array"))
        {
            var wrapped = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject { ["translations"] = responseFormat },
                ["required"] = new JsonArray("translations"),
                ["additionalProperties"] = false,
            };
            responseFormat = wrapped;
        }

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
