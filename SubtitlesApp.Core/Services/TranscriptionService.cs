using System.Runtime.CompilerServices;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.ExternalClients;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Services;

public class TranscriptionService(
    IAudioChunker audioChunker,
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
        var context = string.Empty;
        SubtitleDto? lastEmitted = null;

        await foreach (var audioChunkResult in audioChunker.ChunkAsync(mediaPath, timeInterval, cancellationToken))
        {
            if (audioChunkResult.IsFailure)
            {
                yield return Result<SubtitleDto>.Failure(audioChunkResult.Error);
                yield break;
            }

            var audioChunk = audioChunkResult.Value;

            var subtitlesResult = await subtitlesClient.GetSubsAsync(
                audioChunk.Audio,
                languageCode,
                context,
                cancellationToken
            );

            if (subtitlesResult.IsFailure)
            {
                yield return Result<SubtitleDto>.Failure(subtitlesResult.Error);
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
                if (lastEmitted is not null && subtitle.EndTime - lastEmitted.EndTime < settings.Epsilon)
                {
                    continue;
                }

                yield return Result<SubtitleDto>.Success(subtitle);
                lastEmitted = subtitle;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
        }
    }

    private static void AlignByTime(List<SubtitleDto> subsToAlign, TimeSpan timeOffset)
    {
        foreach (var subtitleDto in subsToAlign)
        {
            subtitleDto.StartTime += timeOffset;
            subtitleDto.EndTime += timeOffset;
        }
    }
}
