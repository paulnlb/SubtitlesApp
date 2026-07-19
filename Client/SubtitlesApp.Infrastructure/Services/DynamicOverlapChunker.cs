using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Infrastructure.Services;

public class DynamicOverlapChunker(IAudioExtractor audioExtractor, TimeSpan chunkLength, TimeSpan maxOverlap)
{
    public async IAsyncEnumerable<Result<AudioChunkDto>> ChunkAsync(
        TimeInterval timeInterval,
        Func<TimeSpan> getAnchor,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        if (chunkLength < TimeSpan.FromSeconds(30))
        {
            yield return Result<AudioChunkDto>.Failure(
                new Error(ErrorCode.InvalidInput, "Audio chunk length must be 30 seconds or longer")
            );
            yield break;
        }

        var subIntervalStart = timeInterval.StartTime;
        var subIntervalEnd = GetEndTime(subIntervalStart, timeInterval.EndTime, chunkLength);

        while (subIntervalStart < timeInterval.EndTime)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield return Result<AudioChunkDto>.Failure(new Error(ErrorCode.OperationCanceled));
                yield break;
            }

            Stream? audioChunk = null;
            Error? extractingError = null;

            try
            {
                audioChunk = await audioExtractor.ExtractAudioAsync(subIntervalStart, subIntervalEnd, cancellationToken);
            }
            catch (Exception ex)
            {
                extractingError = new Error(ErrorCode.InternalClientError, ex.Message);
            }

            if (extractingError is not null)
            {
                yield return Result<AudioChunkDto>.Failure(extractingError);
                yield break;
            }

            yield return Result<AudioChunkDto>.Success(
                new AudioChunkDto
                {
                    StartTime = subIntervalStart,
                    EndTime = subIntervalEnd,
                    Audio = audioChunk!,
                }
            );

            // do not do overlapping if current interval is the last one
            if (subIntervalEnd == timeInterval.EndTime)
            {
                yield break;
            }

            var minStart = subIntervalEnd - maxOverlap;
            var intendedStart = getAnchor();

            subIntervalStart = Max(intendedStart, minStart);
            subIntervalEnd = GetEndTime(subIntervalStart, timeInterval.EndTime, chunkLength);
        }
    }

    private static TimeSpan GetEndTime(TimeSpan startTime, TimeSpan maxEndTime, TimeSpan chunkLength) =>
        maxEndTime <= startTime + chunkLength ? maxEndTime : startTime + chunkLength;

    private static TimeSpan Max(TimeSpan first, TimeSpan second)
    {
        return first >= second ? first : second;
    }
}
