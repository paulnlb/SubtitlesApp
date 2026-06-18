using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Unicode;
using Google.GenAI;
using Google.GenAI.Types;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.ExternalClients;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Mapper;

namespace SubtitlesApp.Infrastructure.ExternalClients;

public class GeminiLlmClient(IGeminiClientSettings settings) : ILlmClient
{
    private readonly Task<Client> _clientTask = InitClient(settings);
    private readonly JsonSchemaExporterOptions _schemaExporterOptions = new() { TreatNullObliviousAsNonNullable = true };

    public async Task<Result<string>> SendChatAsync(
        List<LlmMessageDto> chatHistory,
        string userPrompt,
        JsonNode responseFormat
    )
    {
        var systemMessage = chatHistory.SingleOrDefault(x => x.Role == LlmRoleConstants.System);

        if (systemMessage is null)
        {
            return Result<string>.Failure(new Error(ErrorCode.InvalidInput, "System prompt is required in chat history"));
        }

        var contentList = new List<Content>();
        LlmMapper.ToGeminiContentList(chatHistory, contentList);
        contentList.Add(new Content { Role = "user", Parts = [Part.FromText(userPrompt)] });

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content { Parts = [Part.FromText(systemMessage.Content)] },
        };

        if (responseFormat is JsonNode schema)
        {
            var jsonString = schema.ToJsonString();
            config.ResponseSchema = Schema.FromJson(jsonString);
            config.ResponseMimeType = "application/json";
        }

        GenerateContentResponse response;

        try
        {
            var client = await _clientTask;
            response = await client.Models.GenerateContentAsync(settings.Model, contentList, config);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(
                new Error(ErrorCode.FailedServerResponse, $"LLM response failed with error: {ex.Message}")
            );
        }

        var resultText = response?.Candidates?[0]?.Content?.Parts?[0]?.Text;

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
        var responseFormat = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(T), _schemaExporterOptions);

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

    private static async Task<Client> InitClient(IGeminiClientSettings settings)
    {
        return new Client(apiKey: await settings.GetSecret());
    }
}
