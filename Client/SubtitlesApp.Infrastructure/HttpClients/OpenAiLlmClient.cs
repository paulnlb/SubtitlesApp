using System.ClientModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Unicode;
using OpenAI;
using OpenAI.Responses;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Mapper;

namespace SubtitlesApp.Infrastructure.HttpClients;

#pragma warning disable OPENAI001
public class OpenAiLlmClient(IOpenAiSettings settings) : ILlmClient
{
    private readonly ResponsesClient _responsesClient = InitClient(settings);

    public async Task<Result<string>> SendChatAsync(
        List<LlmMessageDto> chatHistory,
        string userPrompt,
        JsonNode? responseFormat = null
    )
    {
        CreateResponseOptions options = new()
        {
            Model = settings.Model,
            ReasoningOptions = new() { ReasoningEffortLevel = ResponseReasoningEffortLevel.None },
        };

        LlmMapper.ToResponseItems(chatHistory, options.InputItems);
        options.InputItems.Add(ResponseItem.CreateUserMessageItem(userPrompt));

        if (responseFormat is JsonNode schema)
        {
            schema["additionalProperties"] = false;
            schema["type"] = "object";
            var bytes = JsonSerializer.SerializeToUtf8Bytes(schema);
            var binaryData = BinaryData.FromBytes(bytes);

            var textFormat = ResponseTextFormat.CreateJsonSchemaFormat("translation", binaryData, jsonSchemaIsStrict: true);

            options.TextOptions = new ResponseTextOptions { TextFormat = textFormat };
        }

        ResponseResult response;

        try
        {
            response = await _responsesClient.CreateResponseAsync(options);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(
                new Error(ErrorCode.InternalClientError, $"LLM response failed with error: {ex.Message}")
            );
        }

        if (response.Status == ResponseStatus.Failed)
        {
            var error = new Error(ErrorCode.FailedServerResponse, $"LLM response failed with reason: {response.Error}");

            return Result<string>.Failure(error);
        }

        string? resultText = null;

        foreach (ResponseItem item in response.OutputItems)
        {
            if (item is MessageResponseItem message)
            {
                resultText = message.Content?.FirstOrDefault()?.Text;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(resultText))
        {
            return Result<string>.Failure(
                new Error(ErrorCode.FailedServerResponse, "LLM response did not contain any text output")
            );
        }

        return Result<string>.Success(resultText);
    }

    public async Task<Result<T>> SendChatAsync<T>(List<LlmMessageDto> chatHistory, string userPrompt)
    {
        var responseFormat = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(T));

        var result = await SendChatAsync(chatHistory, userPrompt, responseFormat);

        if (result.IsFailure)
        {
            return Result<T>.Failure(result.Error);
        }

        var deserialized = JsonSerializer.Deserialize<T>(
            result.Value,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic, UnicodeRanges.Arabic),
            }
        );

        if (deserialized is null)
        {
            return Result<T>.Failure(new Error(ErrorCode.InternalClientError, "Could not deserialize llm output"));
        }

        return Result<T>.Success(deserialized);
    }

    private static ResponsesClient InitClient(IOpenAiSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            return new(
                new ApiKeyCredential(settings.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint!) }
            );
        }

        return new(settings.ApiKey);
    }
}
