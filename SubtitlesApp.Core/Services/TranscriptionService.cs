using System.Runtime.CompilerServices;
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
        TimeInterval timeInterval,
        string languageCode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var audio = await audioExtractor.ExtractAudioAsync(
                mediaPath,
                timeInterval.StartTime,
                timeInterval.EndTime,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            var transcriptionResult = await subtitlesClient.GetSubsAsync(audio, languageCode, cancellationToken);

            if (transcriptionResult.IsFailure)
            {
                return ListResult<SubtitleDto>.Failure(transcriptionResult.Error);
            }

            if (timeInterval.StartTime != TimeSpan.Zero)
            {
                AlignSubsByTime(transcriptionResult.Value, timeInterval.StartTime);
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

    public async IAsyncEnumerable<ListResult<SubtitleDto>> TranscribeWithSplitAsync(
        string mediaPath,
        TimeInterval timeInterval,
        string languageCode,
        TimeSpan splitTo,
        TimeSpan overlapSize = default,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var subInterval in timeInterval.Split(splitTo, overlapSize))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return await TranscribeAsync(mediaPath, subInterval, languageCode, cancellationToken);
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
