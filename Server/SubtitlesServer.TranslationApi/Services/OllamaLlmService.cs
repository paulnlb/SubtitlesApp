using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using SubtitlesApp.Core.Result;
using SubtitlesServer.TranslationApi.Configs;
using SubtitlesServer.TranslationApi.Interfaces;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Services;

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaConfig _config;
    private readonly IMapper _mapper;
    private readonly ILogger<OllamaLlmService> _logger;

    public OllamaLlmService(
        HttpClient httpClient,
        IOptions<OllamaConfig> ollamaOptions,
        IMapper mapper,
        ILogger<OllamaLlmService> logger
    )
    {
        _httpClient = httpClient;
        _config = ollamaOptions.Value;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<string>> SendAsync(List<LlmMessageDto> chatHistory, string userPrompt)
    {
        var client = new OllamaApiClient(_httpClient, _config.ModelName);

        var pingResult = await PingAsync(client);

        if (pingResult.IsFailure)
        {
            return Result<string>.Failure(pingResult.Error);
        }

        var chat = new Chat(client)
        {
            Options = new OllamaSharp.Models.RequestOptions { Temperature = _config.Temperature, NumCtx = _config.NumCtx },
            Messages = _mapper.Map<List<Message>>(chatHistory),
        };

        var response = new StringBuilder();

        try
        {
            await foreach (var aiMessage in chat.SendAsync(userPrompt))
            {
                response.Append(aiMessage);
            }

            return Result<string>.Success(response.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError("Ollama returned an error: {error}", ex.Message);
            var error = new Error(ErrorCode.BadGateway, ex.Message);
            return Result<string>.Failure(error);
        }
    }

    public AsyncEnumerableResult<string> StreamAsync(List<LlmMessageDto> chatHistory, string userPrompt)
    {
        var client = new OllamaApiClient(_httpClient, _config.ModelName);

        var pingResult = PingAsync(client).Result;

        if (pingResult.IsFailure)
        {
            return AsyncEnumerableResult<string>.Failure(pingResult.Error);
        }

        var chat = new Chat(client)
        {
            Options = new OllamaSharp.Models.RequestOptions { Temperature = _config.Temperature, NumCtx = _config.NumCtx },
            Messages = _mapper.Map<List<Message>>(chatHistory),
        };

        var responsePortions = chat.SendAsync(userPrompt);

        return AsyncEnumerableResult<string>.Success(responsePortions);
    }

    private async Task<Result> PingAsync(OllamaApiClient client)
    {
        try
        {
            if (await client.IsRunningAsync())
            {
                return Result.Success();
            }

            var error = new Error(ErrorCode.BadGateway, "Ai API is not reachable");
            return Result.Failure(error);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ollama API is not reachable. Error: {error}", ex.Message);
            var error = new Error(ErrorCode.BadGateway, "Ai API is not reachable");
            return Result.Failure(error);
        }
    }
}
