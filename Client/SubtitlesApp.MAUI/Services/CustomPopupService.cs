using CommunityToolkit.Maui;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Interfaces;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Services;

public class CustomPopupService(IPopupService toolkitPopupService) : ICustomPopupService
{
    private readonly PopupOptions _popupOptions = new()
    {
        Shape = null,
        Shadow = null,
        CanBeDismissedByTappingOutsideOfPopup = false,
    };

    public async Task<TranscriptionSettings?> ShowTranscriptionSettings(
        TimeSpan mediaDuration,
        TimeSpan currentMediaTime,
        Language language,
        TimeSpan? fromTime,
        TimeSpan? toTime
    )
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(TranscribePopupViewModel.MediaDuration), mediaDuration },
            { nameof(TranscribePopupViewModel.SubtitlesLanguage), language },
            { nameof(TranscribePopupViewModel.Title), "Transcription" },
            { nameof(TranscribePopupViewModel.AcceptText), "Transcribe" },
            { nameof(TranscribePopupViewModel.CurrentMediaTime), currentMediaTime },
        };

        if (fromTime is not null)
        {
            queryAttributes.Add(nameof(TranscribePopupViewModel.FromTime), fromTime);
        }
        if (toTime is not null)
        {
            queryAttributes.Add(nameof(TranscribePopupViewModel.ToTime), toTime);
        }

        var popupResult = await toolkitPopupService.ShowPopupAsync<TranscribePopupViewModel, TranscriptionSettings>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }

    public async Task<TranslationSettings?> ShowTranslationSettings(
        TimeSpan mediaDuration,
        Language? targetLanguage,
        TimeSpan? fromTime,
        TimeSpan? toTime
    )
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(TranslatePopupViewModel.MediaDuration), mediaDuration },
            { nameof(TranslatePopupViewModel.Title), "Translation" },
            { nameof(TranslatePopupViewModel.AcceptText), "Translate" },
        };

        if (targetLanguage is not null)
        {
            queryAttributes.Add(nameof(TranslatePopupViewModel.TargetLanguage), targetLanguage);
        }
        if (fromTime is not null)
        {
            queryAttributes.Add(nameof(TranslatePopupViewModel.FromTime), fromTime);
        }
        if (toTime is not null)
        {
            queryAttributes.Add(nameof(TranslatePopupViewModel.ToTime), toTime);
        }

        var popupResult = await toolkitPopupService.ShowPopupAsync<TranslatePopupViewModel, TranslationSettings>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }

    public async Task<string?> ShowUrlEntry()
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(UrlEntryPopupVm.Title), "Enter Url" },
            { nameof(UrlEntryPopupVm.AcceptText), "Open" },
        };

        var popupResult = await toolkitPopupService.ShowPopupAsync<UrlEntryPopupVm, string>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }

    public async Task<T?> ShowRadioButtons<T>(
        string title,
        IEnumerable<T> sourceItems,
        Func<T, string> displaySelector,
        T? selected,
        string? description = null
    )
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(RadioButtonPopupVm<>.Title), title },
            { nameof(RadioButtonPopupVm<>.SourceItems), sourceItems },
            { nameof(RadioButtonPopupVm<>.DisplaySelector), displaySelector },
            { nameof(RadioButtonPopupVm<>.SelectedItem), selected ?? default },
            { nameof(RadioButtonPopupVm<>.Description), description },
        };

        var popupResult = await toolkitPopupService.ShowPopupAsync<RadioButtonPopupVm<T>, T>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }

    public Task CloseCurrentAsync()
    {
        return toolkitPopupService.ClosePopupAsync(Shell.Current);
    }

    public async Task<T?> CloseCurrentAsync<T>(T result)
    {
        var popupResult = await toolkitPopupService.ClosePopupAsync(Shell.Current, result);

        return popupResult.Result;
    }

    public async Task<string?> ShowEntry(string title, string? value)
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(StringEntryPopupVm.Title), title },
            { nameof(StringEntryPopupVm.AcceptText), "Ok" },
            { nameof(StringEntryPopupVm.Value), value },
        };

        var popupResult = await toolkitPopupService.ShowPopupAsync<StringEntryPopupVm, string>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }

    public async Task<TimeSpan?> ShowTimeEntry(
        string title,
        TimeSpan value,
        TimeSpan? min = null,
        TimeSpan? max = null,
        TimeEntryScope timeScope = TimeEntryScope.Hours,
        IEnumerable<TimePreset>? timePresets = null
    )
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(TimeEntryPopupVm.Title), title },
            { nameof(TimeEntryPopupVm.AcceptText), "Ok" },
            { nameof(TimeEntryPopupVm.Value), value },
            { nameof(TimeEntryPopupVm.Min), min },
            { nameof(TimeEntryPopupVm.Max), max },
            { nameof(TimeEntryPopupVm.TimeScope), timeScope },
            { nameof(TimeEntryPopupVm.Presets), timePresets },
        };

        var popupResult = await toolkitPopupService.ShowPopupAsync<TimeEntryPopupVm, TimeSpan?>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }

    public async Task<int?> ShowCounter(string title, int value, int min = 0, int max = int.MaxValue)
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(CounterPopupVm.Title), title },
            { nameof(CounterPopupVm.AcceptText), "Ok" },
            { nameof(CounterPopupVm.Counter), value },
            { nameof(CounterPopupVm.Min), min },
            { nameof(CounterPopupVm.Max), max },
        };

        var popupResult = await toolkitPopupService.ShowPopupAsync<CounterPopupVm, int?>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }

    public async Task<double?> ShowDoubleEntry(string title, double value, double? min = null, double? max = null)
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(DoubleEntryPopupVm.Title), title },
            { nameof(DoubleEntryPopupVm.AcceptText), "Ok" },
            { nameof(DoubleEntryPopupVm.Value), value },
            { nameof(DoubleEntryPopupVm.Min), min },
            { nameof(DoubleEntryPopupVm.Max), max },
        };

        var popupResult = await toolkitPopupService.ShowPopupAsync<DoubleEntryPopupVm, double?>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }
}
