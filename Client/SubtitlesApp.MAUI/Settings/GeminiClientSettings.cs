using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class GeminiClientSettings : IGeminiClientSettings
{
    private const string _geminiAiModelKey = "gemini_model";
    private const string _geminiDefaultModel = "gemini-3.5-flash";

    private const string _geminiApiKeyKey = "gemini_api_key";
    private const string _geminiDefaultApiKey = " ";

    public string Model
    {
        get => Preferences.Get(_geminiAiModelKey, _geminiDefaultModel);
        set => Preferences.Set(_geminiAiModelKey, value);
    }

    public async Task<string> GetSecret()
    {
        return await SecureStorage.Default.GetAsync(_geminiApiKeyKey) ?? _geminiDefaultApiKey;
    }

    public Task SetSecret(string value)
    {
        return SecureStorage.Default.SetAsync(_geminiApiKeyKey, value);
    }
}
