using Catalyst;
using Microsoft.Extensions.Options;
using Mosaik.Core;
using SubtitlesApp.Core.Constants;
using SubtitlesServer.WhisperApi.Configs;

namespace SubtitlesServer.WhisperApi.Services.ModelProviders;

public class CatalystModelProvider
{
    private readonly Dictionary<string, Lazy<Task<Pipeline>>> _pipelineTasks;
    private readonly CatalystConfig _catalystConfig;

    public CatalystModelProvider(IOptions<CatalystConfig> catalystOptions)
    {
        _catalystConfig = catalystOptions.Value;
        Storage.Current = new DiskStorage(_catalystConfig.BinariesPath);
        _pipelineTasks = [];
        FillPipelineDictionary();
    }

    public Task<Pipeline> GetPipelineAsync(string language, string? fallbackLanguage = null)
    {
        var gotSuccessfully = _pipelineTasks.TryGetValue(language, out var pipelineTask);

        if (!gotSuccessfully && fallbackLanguage != null)
        {
            gotSuccessfully = _pipelineTasks.TryGetValue(fallbackLanguage, out pipelineTask);
        }

        if (!gotSuccessfully)
        {
            throw new InvalidOperationException($"Pipeline for {language} not found");
        }

        return pipelineTask!.Value;
    }

    private void FillPipelineDictionary()
    {
        _pipelineTasks.Add(
            LanguageCodes.English,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.English, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Afrikaans,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Afrikaans, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Arabic,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Arabic, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Armenian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Armenian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Belarusian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Belarusian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Bulgarian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Bulgarian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Catalan,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Catalan, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Chinese,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Chinese, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Croatian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Croatian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Czech,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Czech, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Danish,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Danish, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Dutch,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Dutch, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Estonian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Estonian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Finnish,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Finnish, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.French,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.French, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Galician,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Galician, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.German,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.German, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Greek,
            new Lazy<Task<Pipeline>>(
                () =>
                    Pipeline.FromStoreAsync(Language.Greek_Modern, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Hindi,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Hindi, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Hungarian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Hungarian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Icelandic,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Icelandic, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Indonesian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Indonesian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Italian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Italian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Japanese,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Japanese, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Kazakh,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Kazakh, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Korean,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Korean, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Latvian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Latvian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Lithuanian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Lithuanian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Macedonian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Macedonian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Marathi,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Marathi, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Norwegian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Norwegian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Persian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Persian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Polish,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Polish, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Portuguese,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Portuguese, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Romanian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Romanian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Serbian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Serbian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Slovak,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Slovak, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Slovenian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Slovenian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Spanish,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Spanish, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Swedish,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Swedish, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Tagalog,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Tagalog, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Tamil,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Tamil, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Turkish,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Turkish, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Ukrainian,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Ukrainian, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Urdu,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Urdu, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
        _pipelineTasks.Add(
            LanguageCodes.Vietnamese,
            new Lazy<Task<Pipeline>>(
                () => Pipeline.FromStoreAsync(Language.Vietnamese, _catalystConfig.ModelsVersion, _catalystConfig.ModelsTag)
            )
        );
    }
}
