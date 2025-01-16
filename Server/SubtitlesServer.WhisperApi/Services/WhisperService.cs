using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Services;
using SubtitlesServer.WhisperApi.Interfaces;
using SubtitlesServer.WhisperApi.Models;
using SubtitlesServer.WhisperApi.Services.ModelProviders;
using Whisper.net;

namespace SubtitlesServer.WhisperApi.Services;

public class WhisperService(
    ILogger<WhisperService> logger,
    WhisperModelProvider whisperModelService,
    LanguageService languageService
) : ISpeechToTextService
{
    public async IAsyncEnumerable<SubtitleDto> TranscribeAudioAsync(
        WhisperDto whisperDto,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var factory = await whisperModelService.GetWhisperFactoryAsync();

        if (string.IsNullOrEmpty(whisperDto.LanguageCode))
        {
            whisperDto.LanguageCode = languageService.GetDefaultLanguage().Code;
        }

        var processor = BuildWhisperProcessor(factory, whisperDto);

        try
        {
            var segments = processor.ProcessAsync(whisperDto.AudioStream, cancellationToken);
            logger.LogDebug("Starting transcribing... Audio language: {textLanguage}", whisperDto.LanguageCode);

            await foreach (var segment in segments)
            {
                var subtitle = new SubtitleDto()
                {
                    Text = segment.Text,
                    StartTime = segment.Start,
                    EndTime = segment.End,
                    LanguageCode = segment.Language,
                };

                yield return subtitle;
            }
        }
        finally
        {
            logger.LogDebug("Transcription complete. Disposing the processor...");
            await processor.DisposeAsync();
        }
    }

    private static WhisperProcessor BuildWhisperProcessor(WhisperFactory factory, WhisperDto whisperRequestModel)
    {
        var whisperBuilder = factory.CreateBuilder().WithLanguage(whisperRequestModel.LanguageCode);

        if (whisperRequestModel.MaxSegmentLength > 0)
        {
            whisperBuilder.WithTokenTimestamps().SplitOnWord().WithMaxSegmentLength(whisperRequestModel.MaxSegmentLength);
        }

        return whisperBuilder.Build();
    }
}
