using System.Runtime.CompilerServices;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.ExternalClients;
using SubtitlesApp.Infrastructure.Interfaces;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Models;
using SubtitlesApp.Infrastructure.Services.FfmpegNative;

namespace SubtitlesApp.Infrastructure.Services;

public class WhisperTranscriptionService(OpenAiTranscriptionClent transcriptionsClient, ITranscriptionSettings settings)
    : ITranscriptionService
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
        var context = string.Empty;
        WhisperSubtitle? lastEmitted = null;

        var audioChunker = new FixedSizeChunker(audioExtractor, settings.ChunkLength, settings.OverlapSize);

        await foreach (var audioChunkResult in audioChunker.ChunkAsync(timeInterval, cancellationToken))
        {
            if (audioChunkResult.IsFailure)
            {
                yield return Result<Subtitle>.Failure(audioChunkResult.Error);
                yield break;
            }

            var audioChunk = audioChunkResult.Value;

            var subtitlesResult = await transcriptionsClient.GetSubsAsync(
                audioChunk.Audio,
                languageCode,
                context,
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
            var subtitlesForContext = subtitles.TakeLast(settings.SubtitlesAsPromptCount).Select(x => x.Text);

            context = string.Join(' ', subtitlesForContext);

            foreach (var subtitle in subtitles)
            {
                if (
                    lastEmitted is not null
                    && subtitle.TimeInterval.EndTime - lastEmitted.TimeInterval.EndTime < settings.Epsilon
                )
                {
                    continue;
                }

                yield return Result<Subtitle>.Success(subtitle);
                lastEmitted = subtitle;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                yield return Result<Subtitle>.Failure(new Error(ErrorCode.OperationCanceled));
                yield break;
            }
        }
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
