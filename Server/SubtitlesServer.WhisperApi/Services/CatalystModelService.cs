using Catalyst;
using Mosaik.Core;
using SubtitlesApp.Core.Constants;

namespace SubtitlesServer.WhisperApi.Services;

public class CatalystModelService
{
    private readonly Dictionary<string, Lazy<Task<Pipeline>>> _pipelineTasks;

    public CatalystModelService()
    {
        Storage.Current = new DiskStorage("./Resources/catalyst-models");
        _pipelineTasks = new Dictionary<string, Lazy<Task<Pipeline>>>();
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
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.English, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Afrikaans,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Afrikaans, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Arabic,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Arabic, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Armenian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Armenian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Belarusian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Belarusian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Bulgarian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Bulgarian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Catalan,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Catalan, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Chinese,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Chinese, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Croatian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Croatian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Czech,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Czech, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Danish,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Danish, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Dutch,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Dutch, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Estonian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Estonian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Finnish,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Finnish, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.French,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.French, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Galician,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Galician, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.German,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.German, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Greek,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Greek_Modern, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Hindi,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Hindi, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Hungarian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Hungarian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Icelandic,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Icelandic, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Indonesian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Indonesian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Italian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Italian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Japanese,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Japanese, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Kazakh,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Kazakh, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Korean,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Korean, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Latvian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Latvian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Lithuanian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Lithuanian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Macedonian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Macedonian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Marathi,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Marathi, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Norwegian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Norwegian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Persian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Persian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Polish,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Polish, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Portuguese,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Portuguese, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Romanian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Romanian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Serbian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Serbian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Slovak,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Slovak, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Slovenian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Slovenian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Spanish,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Spanish, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Swedish,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Swedish, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Tagalog,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Tagalog, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Tamil,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Tamil, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Turkish,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Turkish, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Ukrainian,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Ukrainian, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Urdu,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Urdu, 0, "sentence-detection"))
        );
        _pipelineTasks.Add(
            LanguageCodes.Vietnamese,
            new Lazy<Task<Pipeline>>(() => Pipeline.FromStoreAsync(Language.Vietnamese, 0, "sentence-detection"))
        );
    }
}
