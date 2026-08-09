using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Interfaces;

public interface ICustomPopupService
{
    public Task<TranscriptionSettings?> ShowTranscriptionSettings(
        TimeSpan mediaDuration,
        TimeSpan currentMediaTime,
        Language language,
        TimeSpan? fromTime,
        TimeSpan? toTime
    );

    public Task<TranslationSettings?> ShowTranslationSettings(
        TimeSpan mediaDuration,
        Language? targetLanguage,
        TimeSpan? fromTime,
        TimeSpan? toTime
    );

    public Task<string?> ShowUrlEntry();

    public Task<T?> ShowRadioButtons<T>(
        string title,
        IEnumerable<T> sourceItems,
        Func<T, string> displaySelector,
        T? selected,
        string? description = null
    );

    public Task CloseCurrentAsync();

    public Task<T?> CloseCurrentAsync<T>(T result);

    Task<string?> ShowEntry(string title, string? value);

    Task<TimeSpan?> ShowTimeEntry(
        string title,
        TimeSpan value,
        TimeSpan? min = null,
        TimeSpan? max = null,
        TimeEntryScope timeScope = TimeEntryScope.Hours,
        IEnumerable<TimePreset>? timePresets = null
    );

    Task<int?> ShowCounter(string title, int value, int min = 0, int max = int.MaxValue);

    Task<double?> ShowDoubleEntry(string title, double value, double? min = null, double? max = null);
}
