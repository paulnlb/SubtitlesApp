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
}
