using System.Runtime.CompilerServices;
using AutoMapper;
using SubtitlesServer.Shared.Models;
using SubtitlesServer.Shared.Services;
using SubtitlesServer.WhisperApi.Interfaces;
using SubtitlesServer.WhisperApi.Models;
using SubtitlesServer.WhisperApi.Services.ModelProviders;
using Whisper.net;

namespace SubtitlesServer.WhisperApi.Services;

public class WhisperService(
    ILogger<WhisperService> logger,
    WhisperModelProvider whisperModelService,
    LanguageService languageService,
    IMapper mapper
) : ISpeechToTextService
{
    public async IAsyncEnumerable<SubtitleDto> TranscribeAudioAsync(
        TranscriptionRequestDto whisperDto,
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
            logger.LogDebug("Transcription started. Audio language: {textLanguage}", whisperDto.LanguageCode);

            await foreach (var segment in segments)
            {
                yield return mapper.Map<SubtitleDto>(segment);
            }
        }
        finally
        {
            logger.LogDebug("Transcription completed");
            await processor.DisposeAsync();
        }
    }

    private static WhisperProcessor BuildWhisperProcessor(WhisperFactory factory, TranscriptionRequestDto whisperDto)
    {
        var whisperBuilder = factory.CreateBuilder().WithLanguage(whisperDto.LanguageCode);

        if (whisperDto.MaxSegmentLength > 0)
        {
            whisperBuilder.WithTokenTimestamps().SplitOnWord().WithMaxSegmentLength(whisperDto.MaxSegmentLength);
        }

        return whisperBuilder.Build();
    }
}
