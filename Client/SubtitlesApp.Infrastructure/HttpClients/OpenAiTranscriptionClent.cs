using System.ClientModel;
using OpenAI;
using OpenAI.Audio;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Infrastructure.HttpClients;

public class OpenAiTranscriptionClent(ITranscriptionClientSettings settings) : ITranscriptionApiClient
{
    private readonly AudioClient _client = InitClient(settings);

    public async Task<ListResult<SubtitleDto>> GetSubsAsync(
        byte[] audioBytes,
        string languageCode,
        CancellationToken cancellationToken = default
    )
    {
        using var stream = new MemoryStream(audioBytes);
        var transcriptionOptions = new AudioTranscriptionOptions()
        {
            ResponseFormat = AudioTranscriptionFormat.Verbose,
            TimestampGranularities = AudioTimestampGranularities.Segment,
        };

        if (languageCode != LanguageCodes.Auto)
        {
            transcriptionOptions.Language = languageCode;
        }

        AudioTranscription apiResult;

        try
        {
            apiResult = await _client.TranscribeAudioAsync(stream, "audio.wav", transcriptionOptions);
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
                    Text = segment.Text,
                    StartTime = segment.StartTime,
                    EndTime = segment.EndTime,
                }
            );
        }

        return ListResult<SubtitleDto>.Success(subtitles);
    }

    private static AudioClient InitClient(ITranscriptionClientSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            return new(
                settings.Model,
                new ApiKeyCredential(settings.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint!) }
            );
        }

        return new(settings.Model, settings.ApiKey);
    }
}
