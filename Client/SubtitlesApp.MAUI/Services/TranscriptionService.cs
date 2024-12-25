using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;
namespace SubtitlesApp.Services;

public class TranscriptionService(
    IMediaProcessor mediaProcessor,
    ISubtitlesService subtitlesService,
    ITranslationService translationService) : ITranscriptionService
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

    public async Task<Result<List<SubtitleDTO>>> TranscribeAsync(
        string mediaPath,
        TimeInterval timeIntervalToTranscribe,
        SubtitlesSettings subtitlesSettings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var audio = await mediaProcessor.ExtractAudioAsync(
                mediaPath,
                timeIntervalToTranscribe.StartTime,
                timeIntervalToTranscribe.EndTime,
                cancellationToken);

            var subsResult = await subtitlesService.GetSubsAsync(
                audio,
                subtitlesSettings.OriginalLanguage.Code,
                timeIntervalToTranscribe.StartTime,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (subsResult.IsFailure)
            {
                return subsResult;
            }

            var subs = subsResult.Value;

            if (subtitlesSettings.TranslateToLanguage?.Code != null)
            {
                var subsTranslationResult = await translationService.TranslateAsync(
                    subs,
                    subtitlesSettings.TranslateToLanguage.Code,
                    cancellationToken);

                if (subsTranslationResult.IsSuccess)
                {
                    subs = subsTranslationResult.Value;
                }
                else
                {
                    return subsTranslationResult;
                }
            }

            return Result<List<SubtitleDTO>>.Success(subs);
        }
        catch (OperationCanceledException)
        {
            var error = new Error(ErrorCode.OperationCanceled, "Transcription operation has been canceled");
            return Result<List<SubtitleDTO>>.Failure(error);
        }
    }
}
