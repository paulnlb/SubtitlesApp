using System.Collections.ObjectModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Extensions;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Mapper;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.ViewModels;

public partial class SubtitlesViewModel : ObservableObject
{
    #region observable properties

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _subtitles;

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _translations;

    [ObservableProperty]
    private string? _mediaPath;

    [ObservableProperty]
    private int _currentSubtitleIndex = -1;

    [ObservableProperty]
    private int _currentTranslationIndex = -1;

    [ObservableProperty]
    private TimeSpan _mediaDuration;

    [ObservableProperty]
    private bool _isTranscriptionLoading;

    [ObservableProperty]
    private bool _isTranslationLoading;

    #endregion

    #region services

    private readonly ITranslationService _translationService;
    private readonly IPopupService _popupService;
    private readonly ITranscriptionService _transcriptionService;
    private readonly IBuiltInDialogService _builtInDialogService;
    private readonly LanguageService _languageService;

    #endregion

    #region private fields

    private readonly SubtitlesMapper _subtitlesMapper;
    private TranscriptionSettings? _transcriptionSettings;
    private TranslationSettings? _translationSettings;

    #endregion

    public SubtitlesViewModel(
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService,
        SubtitlesMapper subtitlesMapper,
        ITranscriptionService transcriptionService,
        IBuiltInDialogService builtInDialogService
    )
    {
        #region observable properties

        MediaPath = null;
        Subtitles = [];
        Translations = [];

        #endregion

        _translationService = translationService;
        _popupService = popupService;
        _transcriptionService = transcriptionService;
        _builtInDialogService = builtInDialogService;
        _languageService = languageService;

        _subtitlesMapper = subtitlesMapper;
    }

    #region commands

    [RelayCommand]
    public void PositionChanged(TimeSpan currentPosition)
    {
        CurrentSubtitleIndex = FindNewIndex(currentPosition, Subtitles, CurrentSubtitleIndex);
        CurrentTranslationIndex = FindNewIndex(currentPosition, Translations, CurrentTranslationIndex);
    }

    [RelayCommand]
    public async Task Transcribe()
    {
        Dictionary<string, object> queryAttributes;

        if (_transcriptionSettings is null)
        {
            queryAttributes = new Dictionary<string, object>
            {
                [nameof(TranscribePopupViewModel.MediaDuration)] = MediaDuration,
                [nameof(TranscribePopupViewModel.SubtitlesLanguage)] = _languageService.GetDefaultLanguage(),
            };
        }
        else
        {
            queryAttributes = new Dictionary<string, object>
            {
                [nameof(TranscribePopupViewModel.MediaDuration)] = MediaDuration,
                [nameof(TranscribePopupViewModel.SubtitlesLanguage)] = _transcriptionSettings.SubtitlesLanguage,
                [nameof(TranscribePopupViewModel.FromTime)] = _transcriptionSettings.FromTime,
                [nameof(TranscribePopupViewModel.ToTime)] = _transcriptionSettings.ToTime,
            };
        }

        var popupResult = await _popupService.ShowPopupAsync<TranscribePopupViewModel, TranscriptionSettings>(
            Shell.Current,
            new PopupOptions { Shape = null, Shadow = null },
            queryAttributes
        );

        if (popupResult.Result is not TranscriptionSettings newSettings)
        {
            return;
        }

        _transcriptionSettings = newSettings;

        IsTranscriptionLoading = true;

        var results = _transcriptionService.TranscribeAsync(
            MediaPath,
            new TimeInterval(newSettings.FromTime, newSettings.ToTime),
            newSettings.SubtitlesLanguage.Code,
            default
        );

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                await _builtInDialogService.DisplayError(result.Error);

                IsTranscriptionLoading = false;

                return;
            }

            var subtitleDto = result.Value;

            // Workaround that reduces the precision of the timemtapms in order to roughly match seeking precision
            subtitleDto.StartTime = TimeSpan.FromMilliseconds(Math.Round(subtitleDto.StartTime.TotalMilliseconds));
            subtitleDto.EndTime = TimeSpan.FromMilliseconds(Math.Round(subtitleDto.EndTime.TotalMilliseconds));

            Subtitles.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(subtitleDto));
        }

        IsTranscriptionLoading = false;
    }

    [RelayCommand]
    public async Task Translate()
    {
        Dictionary<string, object> queryAttributes;

        if (_translationSettings is null)
        {
            queryAttributes = new Dictionary<string, object>
            {
                [nameof(TranslatePopupViewModel.MediaDuration)] = MediaDuration,
            };
        }
        else
        {
            queryAttributes = new Dictionary<string, object>
            {
                [nameof(TranslatePopupViewModel.MediaDuration)] = MediaDuration,
                [nameof(TranslatePopupViewModel.TargetLanguage)] = _translationSettings.TargetLanguage,
                [nameof(TranslatePopupViewModel.FromTime)] = _translationSettings.FromTime,
                [nameof(TranslatePopupViewModel.ToTime)] = _translationSettings.ToTime,
            };
        }

        var popupResult = await _popupService.ShowPopupAsync<TranslatePopupViewModel, TranslationSettings>(
            Shell.Current,
            new PopupOptions { Shape = null, Shadow = null },
            queryAttributes
        );

        if (popupResult.Result is not TranslationSettings newSettings)
        {
            return;
        }

        _translationSettings = newSettings;

        var subtitlesToTranslate = Subtitles.Where(s =>
            s.TimeInterval.StartTime >= newSettings.FromTime && s.TimeInterval.EndTime <= newSettings.ToTime
        );

        var subtitlesDtos = _subtitlesMapper.VisualSubtitlesToSubtitleDtoList(subtitlesToTranslate);

        IsTranslationLoading = true;

        var results = _translationService.TranslateAsync(subtitlesDtos, newSettings.TargetLanguage, default);

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                await _builtInDialogService.DisplayError(result.Error);

                IsTranslationLoading = false;

                return;
            }

            Translations.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(result.Value));
        }

        IsTranslationLoading = false;
    }

    #endregion

    private static int FindNewIndex(TimeSpan currPosition, ObservableCollection<VisualSubtitle> subtitles, int currIndex)
    {
        if (subtitles is null or { Count: 0 })
        {
            return -1;
        }

        if (currIndex == -1)
        {
            var (_, i) = subtitles.BinarySearch(currPosition);

            return i;
        }

        var currSub = subtitles[currIndex];

        if (currSub.TimeInterval.ContainsTime(currPosition))
        {
            return currIndex;
        }

        VisualSubtitle? prevSub = currIndex > 0 ? subtitles[currIndex - 1] : null;
        VisualSubtitle? nextSub = currIndex < subtitles.Count - 1 ? subtitles[currIndex + 1] : null;

        int newIndex;

        if (prevSub is not null && prevSub.TimeInterval.ContainsTime(currPosition))
        {
            newIndex = currIndex - 1;
        }
        else if (nextSub is not null && nextSub.TimeInterval.ContainsTime(currPosition))
        {
            newIndex = currIndex + 1;
        }
        else
        {
            (_, newIndex) = subtitles.BinarySearch(currPosition);
        }

        return newIndex;
    }
}
