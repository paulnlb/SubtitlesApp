using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Services;

public class TranscriptionService(
    IAudioExtractor audioExtractor,
    ITranscriptionApiClient subtitlesClient,
    ITranscriptionSettings settings
) : ITranscriptionService
{
    public async IAsyncEnumerable<Result<SubtitleDto>> TranscribeAsync(
        string mediaPath,
        TimeInterval timeInterval,
        string languageCode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var subIntervalStart = timeInterval.StartTime;
        var subIntervalEnd = GetEndTime(subIntervalStart, timeInterval.EndTime);

        while (subIntervalStart < timeInterval.EndTime)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            var subtitlesResult = await TranscribeAsyncInternal(
                mediaPath,
                subIntervalStart,
                subIntervalEnd,
                languageCode,
                cancellationToken
            );

            if (subtitlesResult.IsFailure)
            {
                yield return Result<SubtitleDto>.Failure(subtitlesResult.Error);
                yield break;
            }

            subIntervalStart = subIntervalEnd;
            subIntervalEnd = GetEndTime(subIntervalStart, timeInterval.EndTime);

            var subtitles = subtitlesResult.Value;

            // implement overlapping by expanding the sub-interval start time backwards by one subtitle
            // not applicable if there is 0 or 1 subtitle in the sub-interval
            // also not applicable if the current sub-interval is the last one
            if (subtitles.Count > 1 && subIntervalEnd < timeInterval.EndTime)
            {
                subIntervalStart = subtitles.Last().StartTime;
            }

            foreach (var subtitle in subtitles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return Result<SubtitleDto>.Success(subtitle);
            }
        }
    }

    private async Task<ListResult<SubtitleDto>> TranscribeAsyncInternal(
        string mediaPath,
        TimeSpan startTime,
        TimeSpan endTime,
        string languageCode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var audio = await audioExtractor.ExtractAudioAsync(mediaPath, startTime, endTime, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var transcriptionResult = await subtitlesClient.GetSubsAsync(audio, languageCode, cancellationToken);

            if (transcriptionResult.IsFailure)
            {
                return ListResult<SubtitleDto>.Failure(transcriptionResult.Error);
            }

            if (startTime != TimeSpan.Zero)
            {
                AlignSubsByTime(transcriptionResult.Value, startTime);
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

    private TimeSpan GetEndTime(TimeSpan startTime, TimeSpan maxEndTime) =>
        maxEndTime <= startTime + settings.SubIntervalSize ? maxEndTime : startTime + settings.SubIntervalSize;
}
