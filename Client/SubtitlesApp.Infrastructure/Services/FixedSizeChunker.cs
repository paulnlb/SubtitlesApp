using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Services.FfmpegNative;

namespace SubtitlesApp.Infrastructure.Services;

public class FixedSizeChunker(ITranscriptionSettings settings, FfmpegNativeService audioExtractor)
{
    private readonly TimeSpan _chunkLength = settings.ChunkLength;
    private readonly FfmpegNativeService _audioExtractor = audioExtractor;

    public async IAsyncEnumerable<Result<AudioChunkDto>> ChunkAsync(
        string audioPath,
        TimeInterval timeInterval,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        if (_chunkLength < TimeSpan.FromSeconds(30))
        {
            yield return Result<AudioChunkDto>.Failure(
                new Error(ErrorCode.InvalidInput, "Audio chunk length must be 30 seconds or longer")
            );
            yield break;
        }

        var subIntervalStart = timeInterval.StartTime;
        var subIntervalEnd = GetEndTime(subIntervalStart, timeInterval.EndTime);

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
                audioChunk = await _audioExtractor.ExtractAudioAsync(
                    audioPath,
                    subIntervalStart,
                    subIntervalEnd,
                    cancellationToken
                );
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
                subIntervalStart = subIntervalEnd;
            }
            else
            {
                subIntervalStart = subIntervalEnd - settings.OverlapSize;
            }

            subIntervalEnd = GetEndTime(subIntervalStart, timeInterval.EndTime);
        }
    }

    private TimeSpan GetEndTime(TimeSpan startTime, TimeSpan maxEndTime) =>
        maxEndTime <= startTime + _chunkLength ? maxEndTime : startTime + _chunkLength;
}
