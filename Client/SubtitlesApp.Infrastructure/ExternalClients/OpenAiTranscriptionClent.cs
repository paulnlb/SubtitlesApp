using System.ClientModel;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Audio;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.ExternalClients;

public partial class OpenAiTranscriptionClent(
    ITranscriptionClientSettings settings,
    ILogger<OpenAiTranscriptionClent> logger
)
{
    private readonly Task<AudioClient> _audioClientTask = InitClient(settings);

    public async Task<ListResult<WhisperSubtitle>> GetSubsAsync(
        Stream audio,
        string languageCode,
        string prompt,
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

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            transcriptionOptions.Prompt = prompt;
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
                LogSkippedSegment(segment.StartTime, segment.EndTime, segment.Text, apiResult.Language);

                continue;
            }

            LogSegment(segment.StartTime, segment.EndTime, segment.Text, apiResult.Language);

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
        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            return new(settings.Model, await settings.GetSecret());
        }

        var apiKey = await settings.GetSecret();

        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = " ";
        }

        return new(
            settings.Model,
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint!) }
        );
    }

    [LoggerMessage(Level = LogLevel.Trace, Message = "ST: {StartTime}, ET: {EndTime}, Text: {Text}, Lang: {Lang}")]
    private partial void LogSegment(TimeSpan startTime, TimeSpan endTime, string text, string lang);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Skipped because one of the threshold values were exceeded. ST: {StartTime}, ET: {EndTime}, Text: {Text}, Lang: {Lang}"
    )]
    private partial void LogSkippedSegment(TimeSpan startTime, TimeSpan endTime, string text, string lang);
}
