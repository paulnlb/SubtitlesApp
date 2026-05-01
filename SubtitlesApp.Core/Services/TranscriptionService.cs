using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Services;

public class TranscriptionService(IAudioExtractor audioExtractor, ITranscriptionApiClient subtitlesClient)
    : ITranscriptionService
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
            audioExtractor.Dispose();
        }

        _disposed = true;
    }

    public async Task<ListResult<SubtitleDto>> TranscribeAsync(
        string mediaPath,
        TimeInterval timeIntervalToTranscribe,
        string languageCode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var audio = await audioExtractor.ExtractAudioAsync(
                mediaPath,
                timeIntervalToTranscribe.StartTime,
                timeIntervalToTranscribe.EndTime,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            var transcriptionResult = await subtitlesClient.GetSubsAsync(audio, languageCode, cancellationToken);

            if (transcriptionResult.IsFailure)
            {
                return ListResult<SubtitleDto>.Failure(transcriptionResult.Error);
            }

            if (timeIntervalToTranscribe.StartTime != TimeSpan.Zero)
            {
                AlignSubsByTime(transcriptionResult.Value, timeIntervalToTranscribe.StartTime);
            }

            return ListResult<SubtitleDto>.Success(transcriptionResult.Value);
        }
        catch (OperationCanceledException)
        {
            var error = new Error(ErrorCode.OperationCanceled, "Transcription operation has been canceled");
            return ListResult<SubtitleDto>.Failure(error);
        }
        catch (Exception)
        {
            var error = new Error(ErrorCode.InternalClientError, "An unexpected error has occured.");
            return ListResult<SubtitleDto>.Failure(error);
        }
    }

    private static void AlignSubsByTime(List<SubtitleDto> subsToAlign, TimeSpan timeOffset)
    {
        foreach (var subtitleDto in subsToAlign)
        {
            subtitleDto.StartTime += timeOffset;
            subtitleDto.EndTime += timeOffset;
        }
    }
}
