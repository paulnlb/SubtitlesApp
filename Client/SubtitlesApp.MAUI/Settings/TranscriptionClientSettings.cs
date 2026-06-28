using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class TranscriptionClientSettings : ITranscriptionClientSettings
{
    private const string _modelKey = "transcription_model";
    private const string _defaultModel = "whisper-1";

    private const string _apiKeyKey = "transcription_api_key";
    private const string _defaultApiKey = " ";

    private const string _endpointKey = "transcription_endpoint";
    private const string _defaultEndpoint = "";

    private const string _noSpeechProbabilityKey = "no_speech_probability";
    private const string _averageLogProbabilityKey = "average_log_probability";
    private const string _compressionRatioKey = "compression_ratio";

    public string Model
    {
        get => Preferences.Get(_modelKey, _defaultModel);
        set => Preferences.Set(_modelKey, value);
    }
    public string? Endpoint
    {
        get => Preferences.Get(_endpointKey, _defaultEndpoint);
        set => Preferences.Set(_endpointKey, value ?? _defaultEndpoint);
    }
    public float? NoSpeechProbabilityThreshold
    {
        get => Preferences.ContainsKey(_noSpeechProbabilityKey) ? Preferences.Get(_noSpeechProbabilityKey, 0f) : null;
        set
        {
            if (value.HasValue)
            {
                Preferences.Set(_noSpeechProbabilityKey, value.Value);
            }
            else
            {
                Preferences.Remove(_noSpeechProbabilityKey);
            }
        }
    }
    public float? AverageLogProbabilityThreshold
    {
        get => Preferences.ContainsKey(_averageLogProbabilityKey) ? Preferences.Get(_averageLogProbabilityKey, 0f) : null;
        set
        {
            if (value.HasValue)
            {
                Preferences.Set(_averageLogProbabilityKey, value.Value);
            }
            else
            {
                Preferences.Remove(_averageLogProbabilityKey);
            }
        }
    }
    public float? CompressionRatioThreshold
    {
        get => Preferences.ContainsKey(_compressionRatioKey) ? Preferences.Get(_compressionRatioKey, 0f) : null;
        set
        {
            if (value.HasValue)
            {
                Preferences.Set(_compressionRatioKey, value.Value);
            }
            else
            {
                Preferences.Remove(_compressionRatioKey);
            }
        }
    }

    public async Task<string> GetSecret()
    {
        return await SecureStorage.Default.GetAsync(_apiKeyKey) ?? _defaultApiKey;
    }

    public Task SetSecret(string value)
    {
        return SecureStorage.Default.SetAsync(_apiKeyKey, value);
    }
}
