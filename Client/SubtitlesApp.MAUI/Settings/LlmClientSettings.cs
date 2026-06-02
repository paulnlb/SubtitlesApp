using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class LlmClientSettings : ILlmClientSettings
{
    private const string _llmProviderKey = "llm_provider";
    private const string _llmProviderDefault = LlmProviderConstants.OpenAi;

    private const string _openAiModelKey = "openai_model";
    private const string _openAiDefaultModel = "gpt-5.4-nano";

    private const string _openAiApiKeyKey = "openai_api_key";
    private const string _openAiDefaultApiKey = " ";

    private const string _openAiEndpointKey = "openai_endpoint";
    private const string _openAiDefaultEndpoint = "";

    private const string _geminiAiModelKey = "gemini_model";
    private const string _geminiDefaultModel = "gemini-3.1-flash";

    private const string _geminiApiKeyKey = "gemini_api_key";
    private const string _geminiDefaultApiKey = " ";

    public string LlmProvider
    {
        get => Preferences.Get(_llmProviderKey, _llmProviderDefault);
        set => Preferences.Set(_llmProviderKey, value);
    }

    public string OpenAiModel
    {
        get => Preferences.Get(_openAiModelKey, _openAiDefaultModel);
        set => Preferences.Set(_openAiModelKey, value);
    }
    public string? OpenAiEndpoint
    {
        get => Preferences.Get(_openAiEndpointKey, _openAiDefaultEndpoint);
        set => Preferences.Set(_openAiEndpointKey, value ?? _openAiDefaultEndpoint);
    }

    public async Task<string> GetOpenAiApiKey()
    {
        return await SecureStorage.Default.GetAsync(_openAiApiKeyKey) ?? _openAiDefaultApiKey;
    }

    public Task SetOpenAiApiKey(string value)
    {
        return SecureStorage.Default.SetAsync(_openAiApiKeyKey, value);
    }

    public string GeminiModel
    {
        get => Preferences.Get(_geminiAiModelKey, _geminiDefaultModel);
        set => Preferences.Set(_geminiAiModelKey, value);
    }

    public async Task<string> GetGeminiApiKey()
    {
        return await SecureStorage.Default.GetAsync(_geminiApiKeyKey) ?? _geminiDefaultApiKey;
    }

    public Task SetGeminiApiKey(string value)
    {
        return SecureStorage.Default.SetAsync(_geminiApiKeyKey, value);
    }
}
