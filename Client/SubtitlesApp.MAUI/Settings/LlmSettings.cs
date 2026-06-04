using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class LlmSettings : ILlmSettings
{
    private const string _llmProviderKey = "llm_provider";
    private const string _llmProviderDefault = LlmProviderConstants.OpenAi;

    public string LlmProvider
    {
        get => Preferences.Get(_llmProviderKey, _llmProviderDefault);
        set => Preferences.Set(_llmProviderKey, value);
    }
}
