using Catalyst;
using SubtitlesApp.Core.Constants;
using SubtitlesServer.WhisperApi.Interfaces;
using SubtitlesServer.WhisperApi.Services.ModelProviders;

namespace SubtitlesServer.WhisperApi.Services;

public class CatalystNlpService(CatalystModelProvider catalystModelService) : INlpService
{
    public async IAsyncEnumerable<string> SplitToSentences(string text, string languageCode)
    {
        var pipeline = await catalystModelService.GetPipelineAsync(languageCode, fallbackLanguage: LanguageCodes.English);
        var doc = new Document(text, pipeline.Language);
        pipeline.ProcessSingle(doc);

        foreach (var sentence in doc.Spans.Select(sp => sp.Value))
        {
            yield return sentence;
        }
    }
}
