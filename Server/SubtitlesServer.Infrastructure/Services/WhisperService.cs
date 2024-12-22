using Microsoft.Extensions.Logging;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;
using System.Runtime.CompilerServices;

namespace SubtitlesServer.Infrastructure.Services;

public class WhisperService(
    ILogger<WhisperService> logger,
    WhisperModelService whisperModelService,
    LanguageService languageService) : IWhisperService
{
    public async IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        byte[] audioBytes,
        string subtitlesLanguageCode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var factory = await whisperModelService.GetWhisperFactoryAsync();

        if (string.IsNullOrEmpty(subtitlesLanguageCode))
        {
            subtitlesLanguageCode = languageService.GetDefaultLanguage().Code;
        }

        var processor = factory.CreateBuilder()
            .WithLanguage(subtitlesLanguageCode)
            .Build();

        logger.LogInformation("Whisper loaded");

        using var audioStream = new MemoryStream(audioBytes);
        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        logger.LogInformation("Starting transcribing... Audio language: {language}", subtitlesLanguageCode);
        
        try
        {
            await foreach (var result in segments)
            {
                var subtitle = new Subtitle()
                {
                    Text = result.Text,
                    TimeInterval = new TimeInterval(result.Start, result.End),
                    LanguageCode = result.Language
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
