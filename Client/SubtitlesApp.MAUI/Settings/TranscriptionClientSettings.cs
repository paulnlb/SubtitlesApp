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

    public async Task<string> GetApiKey()
    {
        return await SecureStorage.Default.GetAsync(_apiKeyKey) ?? _defaultApiKey;
    }

    public Task SetApiKey(string value)
    {
        return SecureStorage.Default.SetAsync(_apiKeyKey, value);
    }
}
