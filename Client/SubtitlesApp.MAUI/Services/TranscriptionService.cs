using AutoMapper;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class TranscriptionService(
    IMediaProcessor mediaProcessor,
    ISubtitlesService subtitlesService,
    ITranslationService translationService,
    IMapper mapper
) : ITranscriptionService
{
    bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            mediaProcessor.Dispose();
        }

        _disposed = true;
    }

    public async Task<ListResult<Subtitle>> TranscribeAsync(
        string mediaPath,
        TimeInterval timeIntervalToTranscribe,
        SubtitlesSettings subtitlesSettings,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var audio = await mediaProcessor.ExtractAudioAsync(
                mediaPath,
                timeIntervalToTranscribe.StartTime,
                timeIntervalToTranscribe.EndTime,
                cancellationToken
            );

            var transcriptionResult = await subtitlesService.GetSubsAsync(
                audio,
                subtitlesSettings.OriginalLanguage.Code,
                timeIntervalToTranscribe.StartTime,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            if (transcriptionResult.IsFailure)
            {
                return ListResult<Subtitle>.Failure(transcriptionResult.Error);
            }

            var originalSubtitleDtos = transcriptionResult.Value;

            // If translation is not required - return subtitles immidiately
            if (subtitlesSettings.TranslateToLanguage?.Code == null)
            {
                return ListResult<Subtitle>.Success(
                    mapper.Map<List<Subtitle>>(originalSubtitleDtos)
                );
            }

            var subsTranslationResult = await translationService.TranslateAsync(
                originalSubtitleDtos,
                subtitlesSettings.TranslateToLanguage.Code,
                cancellationToken
            );

            if (subsTranslationResult.IsFailure)
            {
                return ListResult<Subtitle>.Failure(transcriptionResult.Error);
            }

            return MapToFinalSubtitles(originalSubtitleDtos, subsTranslationResult.Value);
        }
        catch (OperationCanceledException)
        {
            var error = new Error(
                ErrorCode.OperationCanceled,
                "Transcription operation has been canceled"
            );
            return ListResult<Subtitle>.Failure(error);
        }
    }

    private ListResult<Subtitle> MapToFinalSubtitles(
        List<SubtitleDto> originalDtos,
        List<SubtitleDto> translatedDtos
    )
    {
        if (originalDtos.Count != translatedDtos.Count)
        {
            var error = new Error(
                ErrorCode.Unspecified,
                $"Sizes of source and translated subtitles lists do not match (original: {originalDtos.Count}, translated: {translatedDtos.Count})"
            );
            return ListResult<Subtitle>.Failure(error);
        }

        var finalSubtites = new List<Subtitle>();

        for (int i = 0; i < originalDtos.Count; i++)
        {
            var finalSubtitle = mapper.Map<Subtitle>(originalDtos[i]);

            finalSubtitle.Translation = new Translation
            {
                LanguageCode = translatedDtos[i].LanguageCode,
                Text = translatedDtos[i].Text,
            };

            finalSubtites.Add(finalSubtitle);
        }

        return ListResult<Subtitle>.Success(finalSubtites);
    }
}
