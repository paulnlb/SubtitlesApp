using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Infrastructure.Configs;

namespace SubtitlesServer.Infrastructure.Services;

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaConfig _config;
    private readonly IMapper _mapper;

    public OllamaLlmService(
        HttpClient httpClient,
        IOptions<OllamaConfig> ollamaOptions,
        IMapper mapper
    )
    {
        _httpClient = httpClient;
        _config = ollamaOptions.Value;
        _mapper = mapper;
    }

    public async Task<string> SendAsync(List<LlmMessageDto> chatHistory, string userPrompt)
    {
        var client = new OllamaApiClient(_httpClient, _config.ModelName);

        var chat = new Chat(client)
        {
            Options = new OllamaSharp.Models.RequestOptions
            {
                Temperature = _config.Temperature,
                NumCtx = _config.NumCtx,
            },
            Messages = _mapper.Map<List<Message>>(chatHistory)
        };

        var response = new StringBuilder();

        await foreach (var aiMessage in chat.SendAsync(userPrompt))
        {
            response.Append(aiMessage);
        }

        return response.ToString();
    }
}
