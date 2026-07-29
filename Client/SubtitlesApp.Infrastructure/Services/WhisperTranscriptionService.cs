using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.ExternalClients;
using SubtitlesApp.Infrastructure.Interfaces;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Models;
using SubtitlesApp.Infrastructure.Services.FfmpegNative;

namespace SubtitlesApp.Infrastructure.Services;

public partial class WhisperTranscriptionService(
    OpenAiTranscriptionClent transcriptionsClient,
    ITranscriptionSettings settings,
    ILogger<WhisperTranscriptionService> logger
) : ITranscriptionService
{
    public IAsyncEnumerable<Result<Subtitle>> TranscribeAsync(
        string mediaPath,
        TimeInterval timeInterval,
        string languageCode,
        CancellationToken cancellationToken = default
    )
    {
        var audioExtractor = new FfmpegAudioExtractor(mediaPath);

        return TranscribeAsync(audioExtractor, timeInterval, languageCode, cancellationToken);
    }

    public IAsyncEnumerable<Result<Subtitle>> TranscribeAsync(
        Stream media,
        TimeInterval timeInterval,
        string languageCode,
        CancellationToken cancellationToken = default
    )
    {
        var audioExtractor = new FfmpegAudioExtractorStream(media);

        return TranscribeAsync(audioExtractor, timeInterval, languageCode, cancellationToken);
    }

    private async IAsyncEnumerable<Result<Subtitle>> TranscribeAsync(
        IAudioExtractor audioExtractor,
        TimeInterval timeInterval,
        string languageCode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        List<WhisperSubtitle> buffer = [];
        TimeSpan bufferChunkEnd = TimeSpan.Zero;
        TimeSpan GetAnchor()
        {
            if (buffer.Count == 0)
            {
                return TimeSpan.Zero;
            }
            else if (buffer.Last().TimeInterval.EndTime < bufferChunkEnd)
            {
                return bufferChunkEnd;
            }
            else
            {
                return buffer.Last().TimeInterval.StartTime;
            }
        }

        var audioChunker = new DynamicOverlapChunker(audioExtractor, settings.ChunkLength, settings.OverlapSize);

        await foreach (var audioChunkResult in audioChunker.ChunkAsync(timeInterval, GetAnchor, cancellationToken))
        {
            if (audioChunkResult.IsFailure)
            {
                yield return Result<Subtitle>.Failure(audioChunkResult.Error);
                yield break;
            }

            var audioChunk = audioChunkResult.Value;

            LogAudioChunk(audioChunk.StartTime, audioChunk.EndTime);

            var prompt =
                settings.SubtitlesAsPromptCount > 0 ? ConstuctDynamicPrompt(buffer, audioChunk.StartTime) : string.Empty;

            LogPrompt(prompt);

            var subtitlesResult = await transcriptionsClient.GetSubsAsync(
                audioChunk.Audio,
                languageCode,
                prompt,
                cancellationToken
            );

            if (subtitlesResult.IsFailure)
            {
                yield return Result<Subtitle>.Failure(subtitlesResult.Error);
                yield break;
            }

            var subtitles = subtitlesResult.Value;

            if (audioChunk.StartTime != TimeSpan.Zero)
            {
                AlignByTime(subtitles, audioChunk.StartTime);
            }

            if (subtitles.Count > 0)
            {
                LogSubsRange(subtitles.First().TimeInterval.StartTime, subtitles.Last().TimeInterval.EndTime);
            }
            else
            {
                logger.LogDebug("No subtitles have been generated");
            }

            if (subtitles.Count > 0 && subtitles.Last().TimeInterval.EndTime > audioChunk.EndTime)
            {
                var removedCount = ClampSubtitles(subtitles, audioChunk.EndTime);
                LogRemovedExtra(removedCount);
            }

            if (buffer.Count == 0)
            {
                buffer = subtitles;
                bufferChunkEnd = audioChunk.EndTime;
                continue;
            }

            buffer.RemoveAll(s => s.TimeInterval.IsLaterOrStartsWith(audioChunk.StartTime));

            foreach (var item in buffer)
            {
                yield return Result<Subtitle>.Success(item);
            }

            buffer = subtitles;
            bufferChunkEnd = audioChunk.EndTime;

            if (cancellationToken.IsCancellationRequested)
            {
                yield return Result<Subtitle>.Failure(new Error(ErrorCode.OperationCanceled));
                yield break;
            }
        }

        foreach (var item in buffer)
        {
            yield return Result<Subtitle>.Success(item);
        }
    }

    private string ConstuctDynamicPrompt(List<WhisperSubtitle> previousSubtitles, TimeSpan chunkStart)
    {
        var subtitlesForPrompt = previousSubtitles
            .Where(s => s.TimeInterval.EndTime <= chunkStart)
            .TakeLast(settings.SubtitlesAsPromptCount);

        string prompt;

        if (!subtitlesForPrompt.Any() || subtitlesForPrompt.Last().TimeInterval.EndTime < chunkStart)
        {
            prompt = string.Empty;
        }
        else
        {
            prompt = string.Join(' ', subtitlesForPrompt.Select(x => x.Text));
        }

        return prompt;
    }

    private static void AlignByTime(List<WhisperSubtitle> subsToAlign, TimeSpan timeOffset)
    {
        foreach (var subtitle in subsToAlign)
        {
            var newStart = subtitle.TimeInterval.StartTime + timeOffset;
            var newEnd = subtitle.TimeInterval.EndTime + timeOffset;

            subtitle.TimeInterval = new TimeInterval(newStart, newEnd);
        }
    }

    private static int ClampSubtitles(List<WhisperSubtitle> subtitles, TimeSpan maxEndTime)
    {
        var removeFrom = -1;

        for (int i = subtitles.Count - 1; i >= 0; i--)
        {
            var sub = subtitles[i];

            if (sub.TimeInterval.StartTime >= maxEndTime)
            {
                removeFrom = i;
            }
            else if (sub.TimeInterval.StartTime < maxEndTime && sub.TimeInterval.EndTime > maxEndTime)
            {
                sub.TimeInterval = new TimeInterval(sub.TimeInterval.StartTime, maxEndTime);
            }
            else
            {
                break;
            }
        }

        if (removeFrom == -1)
        {
            return 0;
        }

        var removedCount = subtitles.Count - removeFrom;
        subtitles.RemoveRange(removeFrom, removedCount);

        return removedCount;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Audio chunk created. Start time: {StartTime}. End Time: {EndTime}")]
    private partial void LogAudioChunk(TimeSpan startTime, TimeSpan endTime);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Prompt for the upcoming subtitle generation: {Prompt}")]
    private partial void LogPrompt(string prompt);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Subtitiles have been gererated. Earliest subtitle ST: {StartTime}. Latest subtitle ET: {EndTime}"
    )]
    private partial void LogSubsRange(TimeSpan startTime, TimeSpan endTime);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Removed {Count} subtitles because their time intervals were outside the audio chunk's time range. If the message says \"Removed 0\", it means the ET of the last subtitle was outside the range and has been adjusted"
    )]
    private partial void LogRemovedExtra(int count);
}
