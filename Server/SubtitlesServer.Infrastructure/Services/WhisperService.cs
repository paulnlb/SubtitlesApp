using Microsoft.Extensions.Logging;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Infrastructure.Services;

public class WhisperService(
    ILogger<WhisperService> logger,
    WhisperModelService whisperModelService,
    LanguageService languageService
) : ITranscriptionService
{
    public async Task<List<SubtitleDto>> TranscribeAudioAsync(
        byte[] audioBytes,
        string subtitlesLanguageCode,
        CancellationToken cancellationToken = default
    )
    {
        var factory = await whisperModelService.GetWhisperFactoryAsync();

        if (string.IsNullOrEmpty(subtitlesLanguageCode))
        {
            subtitlesLanguageCode = languageService.GetDefaultLanguage().Code;
        }

        var processor = factory.CreateBuilder().WithLanguage(subtitlesLanguageCode).Build();

        logger.LogInformation("Whisper loaded");

        using var audioStream = new MemoryStream(audioBytes);
        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        logger.LogInformation(
            "Starting transcribing... Audio language: {language}",
            subtitlesLanguageCode
        );

        var subtitles = new List<SubtitleDto>();

        try
        {
            await foreach (var result in segments)
            {
                var subtitle = new SubtitleDto()
                {
                    Text = result.Text,
                    StartTime = result.Start,
                    EndTime = result.End,
                    LanguageCode = result.Language,
                };

                AddOrMergeWithLast(subtitles, subtitle);
            }

            return subtitles;
        }
        finally
        {
            await processor.DisposeAsync();
        }
    }

    private static void AddOrMergeWithLast(
        List<SubtitleDto> subtitleDtos,
        SubtitleDto subtitleDtoToAdd
    )
    {
        var lastSubtitleDto = subtitleDtos.LastOrDefault();

        if (lastSubtitleDto?.Text == subtitleDtoToAdd.Text)
        {
            lastSubtitleDto.EndTime = subtitleDtoToAdd.EndTime;
        }
        else
        {
            subtitleDtos.Add(subtitleDtoToAdd);
        }
    }
}
