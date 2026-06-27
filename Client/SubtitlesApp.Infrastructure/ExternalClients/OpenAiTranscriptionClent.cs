using System.ClientModel;
using OpenAI;
using OpenAI.Audio;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Infrastructure.ExternalClients;

public class OpenAiTranscriptionClent(ITranscriptionClientSettings settings)
{
    private readonly Task<AudioClient> _audioClientTask = InitClient(settings);

    public async Task<ListResult<SubtitleDto>> GetSubsAsync(
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
            apiResult = await audioClient.TranscribeAudioAsync(audio, "audio.wav", transcriptionOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            return ListResult<SubtitleDto>.Failure(
                new Error(ErrorCode.InternalClientError, $"Audio transcription failed with error: {ex.Message}")
            );
        }

        var subtitles = new List<SubtitleDto>();

        foreach (TranscribedSegment segment in apiResult.Segments)
        {
            subtitles.Add(
                new()
                {
                    LanguageCode = apiResult.Language,
                    Text = segment.Text.TrimStart(),
                    StartTime = segment.StartTime,
                    EndTime = segment.EndTime,
                }
            );
        }

        return ListResult<SubtitleDto>.Success(subtitles);
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
