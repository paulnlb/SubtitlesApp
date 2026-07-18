using System.ClientModel;
using OpenAI;
using OpenAI.Audio;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.ExternalClients;

public class OpenAiTranscriptionClent(ITranscriptionClientSettings settings)
{
    private readonly Task<AudioClient> _audioClientTask = InitClient(settings);

    public async Task<ListResult<WhisperSubtitle>> GetSubsAsync(
        Stream audio,
        string languageCode,
        string context,
        CancellationToken cancellationToken = default
    )
    {
        var transcriptionOptions = new AudioTranscriptionOptions()
        {
            ResponseFormat = AudioTranscriptionFormat.Verbose,
            TimestampGranularities = AudioTimestampGranularities.Segment,
        };

        if (languageCode != LanguageCodes.Auto)
        {
            transcriptionOptions.Language = languageCode;
        }

        if (!string.IsNullOrWhiteSpace(context))
        {
            transcriptionOptions.Prompt = context;
        }

        AudioTranscription apiResult;

        try
        {
            var audioClient = await _audioClientTask;

            // cancellation token is not forwarded because of inconsistent behavior during cancellation
            // (main thread is blocked and sometimes 'Socket Closed' exception is thrown instead of OperationCancelledException)
            apiResult = await audioClient.TranscribeAudioAsync(audio, "audio.wav", transcriptionOptions);
        }
        catch (Exception ex)
        {
            return ListResult<WhisperSubtitle>.Failure(
                new Error(ErrorCode.InternalClientError, $"Audio transcription failed with error: {ex.Message}")
            );
        }

        var subtitles = new List<WhisperSubtitle>();

        foreach (TranscribedSegment segment in apiResult.Segments)
        {
            if (
                segment.NoSpeechProbability > settings.NoSpeechProbabilityThreshold
                || segment.AverageLogProbability < settings.AverageLogProbabilityThreshold
                || segment.CompressionRatio > settings.CompressionRatioThreshold
            )
            {
                continue;
            }

            subtitles.Add(
                new()
                {
                    LanguageCode = apiResult.Language,
                    Text = segment.Text.TrimStart(),
                    TimeInterval = new TimeInterval(segment.StartTime, segment.EndTime),
                    NoSpeechProbability = segment.NoSpeechProbability,
                    AverageLogProbability = segment.AverageLogProbability,
                    CompressionRatio = segment.CompressionRatio,
                }
            );
        }

        return ListResult<WhisperSubtitle>.Success(subtitles);
    }

    private static async Task<AudioClient> InitClient(ITranscriptionClientSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            return new(
                settings.Model,
                new ApiKeyCredential(await settings.GetSecret()),
                new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint!) }
            );
        }

        return new(settings.Model, await settings.GetSecret());
    }
}
