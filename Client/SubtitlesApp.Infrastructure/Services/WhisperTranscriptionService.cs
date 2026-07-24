using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.ExternalClients;
using SubtitlesApp.Infrastructure.Interfaces;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Models;
using SubtitlesApp.Infrastructure.Services.FfmpegNative;

namespace SubtitlesApp.Infrastructure.Services;

public class WhisperTranscriptionService(
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
        TimeSpan getAnchor()
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

        await foreach (var audioChunkResult in audioChunker.ChunkAsync(timeInterval, getAnchor, cancellationToken))
        {
            if (audioChunkResult.IsFailure)
            {
                yield return Result<Subtitle>.Failure(audioChunkResult.Error);
                yield break;
            }

            var audioChunk = audioChunkResult.Value;

            logger.LogDebug(
                "Audio chunk created. Start time: {StartTime}. End Time: {EndTime}",
                audioChunk.StartTime,
                audioChunk.EndTime
            );

            var prompt =
                settings.SubtitlesAsPromptCount > 0 ? ConstuctDynamicPrompt(buffer, audioChunk.StartTime) : string.Empty;

            logger.LogDebug("Prompt for the upcoming subtitle generation: {Prompt}", prompt);

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

            if (audioChunk.StartTime != TimeSpan.Zero)
            {
                AlignByTime(subtitlesResult.Value, audioChunk.StartTime);
            }

            var subtitles = subtitlesResult.Value;

            if (subtitles.Count > 0)
            {
                logger.LogDebug(
                    "Subtitiles gererated. Earliest subtitle start time: {StartTime}. Latest subtitle end Time: {EndTime}",
                    subtitles.First().TimeInterval.StartTime,
                    subtitles.Last().TimeInterval.EndTime
                );
            }
            else
            {
                logger.LogDebug("No Subtitles were generated");
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
}
