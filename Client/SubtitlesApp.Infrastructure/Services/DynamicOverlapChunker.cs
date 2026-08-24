using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Infrastructure.Services;

public class DynamicOverlapChunker(IMediaProcessingService audioExtractor, TimeSpan chunkLength, TimeSpan maxOverlap)
{
    public async IAsyncEnumerable<Result<AudioChunkDto>> ChunkAsync(
        string mediaPath,
        TimeInterval timeInterval,
        Func<TimeSpan> getAnchor,
        int audioTrackIndex = 0,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
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
                yield return Result<AudioChunkDto>.Failure(new Error(ErrorCode.OperationCancelled));
                yield break;
            }

            var result = await audioExtractor.ExtractAudioAsync(
                mediaPath,
                subIntervalStart,
                subIntervalEnd,
                audioTrackIndex,
                cancellationToken
            );

            if (result.IsFailure)
            {
                yield return Result<AudioChunkDto>.Failure(result.Error);
                yield break;
            }

            yield return Result<AudioChunkDto>.Success(
                new AudioChunkDto
                {
                    StartTime = subIntervalStart,
                    EndTime = subIntervalEnd,
                    Audio = result.Value,
                }
            );

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
