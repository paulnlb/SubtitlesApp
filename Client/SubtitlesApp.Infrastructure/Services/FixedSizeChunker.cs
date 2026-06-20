using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Infrastructure.Services;

public class FixedSizeChunker : IAudioChunker
{
    private readonly TimeSpan _chunkLength;
    private readonly IAudioExtractor _audioExtractor;

    public FixedSizeChunker(ITranscriptionSettings settings, IAudioExtractor audioExtractor)
    {
        _chunkLength = settings.ChunkLength;
        _audioExtractor = audioExtractor;
    }

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
            catch (Exception)
            {
                extractingError = new Error(ErrorCode.InternalClientError, "An unexpected error has occured.");
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

            subIntervalStart = subIntervalEnd;
            subIntervalEnd = GetEndTime(subIntervalStart, timeInterval.EndTime);
        }
    }

    private TimeSpan GetEndTime(TimeSpan startTime, TimeSpan maxEndTime) =>
        maxEndTime <= startTime + _chunkLength ? maxEndTime : startTime + _chunkLength;
}
