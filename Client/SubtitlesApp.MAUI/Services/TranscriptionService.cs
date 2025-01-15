using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class TranscriptionService(IMediaProcessor mediaProcessor, ISubtitlesService subtitlesService) : ITranscriptionService
{
    private bool _disposed;

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

    public async Task<ListResult<SubtitleDto>> TranscribeAsync(
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
                return ListResult<SubtitleDto>.Failure(transcriptionResult.Error);
            }

            return ListResult<SubtitleDto>.Success(transcriptionResult.Value);
        }
        catch (OperationCanceledException)
        {
            var error = new Error(ErrorCode.OperationCanceled, "Transcription operation has been canceled");
            return ListResult<SubtitleDto>.Failure(error);
        }
    }
}
