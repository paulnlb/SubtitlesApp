using CommunityToolkit.Maui;
using SubtitlesApp.ClientModels;
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
        T? selected
    )
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(RadioButtonPopupVm<>.Title), title },
            { nameof(RadioButtonPopupVm<>.SourceItems), sourceItems },
            { nameof(RadioButtonPopupVm<>.DisplaySelector), displaySelector },
            { nameof(RadioButtonPopupVm<>.SelectedItem), selected ?? default },
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

    public async Task<TimeSpan> ShowTimeEntry(string title, TimeSpan value)
    {
        var queryAttributes = new Dictionary<string, object>
        {
            { nameof(StringEntryPopupVm.Title), title },
            { nameof(StringEntryPopupVm.AcceptText), "Ok" },
            { nameof(StringEntryPopupVm.Value), value },
        };

        var popupResult = await toolkitPopupService.ShowPopupAsync<TimeEntryPopupVm, TimeSpan>(
            Shell.Current,
            _popupOptions,
            queryAttributes
        );

        return popupResult.Result;
    }
}
