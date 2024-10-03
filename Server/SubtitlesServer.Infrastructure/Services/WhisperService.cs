using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitlesApp.Core.Models;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Infrastructure.Configs;
using System.Runtime.CompilerServices;

namespace SubtitlesServer.Infrastructure.Services;

public class WhisperService(
    ILogger<WhisperService> logger,
    IOptions<SpeechToTextConfigs> speechToTextConfigs,
    WhisperModelService whisperModelService) : IWhisperService
{
    public async IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        byte[] audioBytes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var language = speechToTextConfigs.Value.Language;
        var factory = await whisperModelService.GetWhisperFactoryAsync();

        var processor = factory.CreateBuilder()
            .WithLanguage(language)
            .Build();

        logger.LogInformation("Whisper loaded");

        using var audioStream = new MemoryStream(audioBytes);
        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        logger.LogInformation("Starting transcribing...");
        
        try
        {
            await foreach (var result in segments)
            {
                var subtitle = new Subtitle()
                {
                    Text = result.Text,
                    TimeInterval = new TimeInterval(result.Start, result.End),
                };

                yield return subtitle;
            }
        }
        finally
        {
            await processor.DisposeAsync();
        }
    }
}
