using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Interfaces;

public interface ICustomPopupService
{
    public Task<TranscriptionSettings?> ShowTranscriptionSettings(
        TimeSpan mediaDuration,
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
        T? selected
    );

    public Task CloseCurrentAsync();

    public Task<T?> CloseCurrentAsync<T>(T result);

    Task<string?> ShowEntry(string title, string? value);

    Task<TimeSpan?> ShowTimeEntry(string title, TimeSpan value);

    Task<int?> ShowCounter(string title, int value, int min = 0, int max = int.MaxValue);
}
