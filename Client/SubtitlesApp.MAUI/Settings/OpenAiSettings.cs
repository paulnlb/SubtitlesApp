using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class OpenAiSettings : IOpenAiSettings
{
    private const string _modelKey = "openai_model";
    private const string _defaultModel = "gpt-5.4-nano";

    private const string _apiKeyKey = "openai_api_key";
    private const string _defaultApiKey = "samplekey";

    private const string _endpointKey = "openai_endpoint";
    private const string _defaultEndpoint = "";

    public string Model
    {
        get => Preferences.Get(_modelKey, _defaultModel);
        set => Preferences.Set(_modelKey, value);
    }
    public string ApiKey
    {
        get => Preferences.Get(_apiKeyKey, _defaultApiKey);
        set => Preferences.Set(_apiKeyKey, value);
    }
    public string? Endpoint
    {
        get => Preferences.Get(_endpointKey, _defaultEndpoint);
        set => Preferences.Set(_endpointKey, value ?? _defaultEndpoint);
    }
}
