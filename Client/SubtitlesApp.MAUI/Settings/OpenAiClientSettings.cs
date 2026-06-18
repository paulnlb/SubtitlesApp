using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class OpenAiClientSettings : IOpenAiClientSettings
{
    private const string _openAiModelKey = "openai_model";
    private const string _openAiDefaultModel = "gpt-5.4-nano";

    private const string _openAiApiKeyKey = "openai_api_key";
    private const string _openAiDefaultApiKey = " ";

    private const string _openAiEndpointKey = "openai_endpoint";
    private const string _openAiDefaultEndpoint = "";

    public string Model
    {
        get => Preferences.Get(_openAiModelKey, _openAiDefaultModel);
        set => Preferences.Set(_openAiModelKey, value);
    }
    public string? Endpoint
    {
        get => Preferences.Get(_openAiEndpointKey, _openAiDefaultEndpoint);
        set => Preferences.Set(_openAiEndpointKey, value ?? _openAiDefaultEndpoint);
    }

    public async Task<string> GetSecret()
    {
        return await SecureStorage.Default.GetAsync(_openAiApiKeyKey) ?? _openAiDefaultApiKey;
    }

    public Task SetSecret(string value)
    {
        return SecureStorage.Default.SetAsync(_openAiApiKeyKey, value);
    }
}
