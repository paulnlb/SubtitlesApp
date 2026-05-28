using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Adapters;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Extensions;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Mapper;
using SubtitlesApp.Messages;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.ViewModels;

public partial class PlayerWithSubtitlesViewModel : ObservableObject, IQueryAttributable
{
    #region observable properties

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _subtitles;

    [ObservableProperty]
    private ObservableCollectionAdapter<VisualSubtitle> _subtitlesAdapter;

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _translations;

    [ObservableProperty]
    private ObservableCollectionAdapter<VisualSubtitle> _translationsAdapter;

    [ObservableProperty]
    private string? _mediaPath;

    [ObservableProperty]
    private SubtitlesCollectionState _subtitlesCollectionState;

    [ObservableProperty]
    private SubtitlesCollectionState _translationsCollectionState;

    [ObservableProperty]
    private bool _playerControlsVisible;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isSubtitlesSelected;

    [ObservableProperty]
    private bool _isTranslationsSelected;

    [ObservableProperty]
    private TimeSpan _mediaDuration;

    [ObservableProperty]
    private double _playerRelativeVerticalLength;

    [ObservableProperty]
    private double _playerRelativeHorizontalLength;

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

    #region events
    public event EventHandler? SubsScrollRequested;
    public event EventHandler? TranslationsScrollRequested;

    #endregion

    #region private properties

    private bool IsCurrentSubVisible =>
        SubtitlesCollectionState.CurrentSubtitleIndex <= SubtitlesCollectionState.LastVisibleSubtitleIndex
        && SubtitlesCollectionState.CurrentSubtitleIndex >= SubtitlesCollectionState.FirstVisibleSubtitleIndex;

    private bool IsCurrentTranslationVisible =>
        TranslationsCollectionState.CurrentSubtitleIndex <= TranslationsCollectionState.LastVisibleSubtitleIndex
        && TranslationsCollectionState.CurrentSubtitleIndex >= TranslationsCollectionState.FirstVisibleSubtitleIndex;

    #endregion

    public PlayerWithSubtitlesViewModel(
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService,
        SubtitlesMapper subtitlesMapper,
        ITranscriptionService transcriptionService,
        IBuiltInDialogService builtInDialogService
    )
    {
        #region observable properties

        PlayerControlsVisible = true;
        MediaPath = null;
        Subtitles = [];
        Translations = [];
        SubtitlesAdapter = new ObservableCollectionAdapter<VisualSubtitle>(_subtitles);
        TranslationsAdapter = new ObservableCollectionAdapter<VisualSubtitle>(_translations);
        SubtitlesCollectionState = new SubtitlesCollectionState { AutoScrollEnabled = true };
        TranslationsCollectionState = new SubtitlesCollectionState { AutoScrollEnabled = true };
        IsSubtitlesSelected = true;
        IsTranslationsSelected = false;

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
        var isSubUpdated = UpdateCurrentSubtitleIndex(currentPosition, Subtitles, SubtitlesCollectionState);

        var isTranslationUpdated = UpdateCurrentSubtitleIndex(currentPosition, Translations, TranslationsCollectionState);

        if (isTranslationUpdated && TranslationsCollectionState.AutoScrollEnabled)
        {
            TranslationsScrollRequested?.Invoke(this, EventArgs.Empty);
        }
        if (isSubUpdated && SubtitlesCollectionState.AutoScrollEnabled)
        {
            SubsScrollRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    public void ScrollToCurentSub()
    {
        if (!IsCurrentSubVisible)
        {
            SubsScrollRequested?.Invoke(this, EventArgs.Empty);
        }

        SubtitlesCollectionState.AutoScrollEnabled = true;
    }

    [RelayCommand]
    public void ScrollToCurentTranslation()
    {
        if (!IsCurrentTranslationVisible)
        {
            TranslationsScrollRequested?.Invoke(this, EventArgs.Empty);
        }

        TranslationsCollectionState.AutoScrollEnabled = true;
    }

    [RelayCommand]
    public void SubtitleTapped(VisualSubtitle subtitle)
    {
        StrongReferenceMessenger.Default.Send(new SeekToPositionMessage(subtitle.TimeInterval.StartTime));
    }

    [RelayCommand]
    public void TogglePlayerControlsVisibility()
    {
        PlayerControlsVisible = !PlayerControlsVisible;
    }

    [RelayCommand]
    public async Task Transcribe()
    {
        object? popupResult;

        if (_transcriptionSettings is null)
        {
            popupResult = await _popupService.ShowPopupAsync<TranscribePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.SubtitlesLanguage = _languageService.GetDefaultLanguage();
            });
        }
        else
        {
            popupResult = await _popupService.ShowPopupAsync<TranscribePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.SubtitlesLanguage = _transcriptionSettings.SubtitlesLanguage;
                vm.FromTime = _transcriptionSettings.FromTime;
                vm.ToTime = _transcriptionSettings.ToTime;
            });
        }

        if (popupResult is not TranscriptionSettings newSettings)
        {
            return;
        }

        _transcriptionSettings = newSettings;

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

                return;
            }

            Subtitles.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(result.Value));
        }
    }

    [RelayCommand]
    public async Task Translate()
    {
        object? popupResult;

        if (_translationSettings is null)
        {
            popupResult = await _popupService.ShowPopupAsync<TranslatePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
            });
        }
        else
        {
            popupResult = await _popupService.ShowPopupAsync<TranslatePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.TargetLanguage = _translationSettings.TargetLanguage;
                vm.FromTime = _translationSettings.FromTime;
                vm.ToTime = _translationSettings.ToTime;
            });
        }

        if (popupResult is not TranslationSettings newSettings)
        {
            return;
        }

        _translationSettings = newSettings;

        var subtitlesToTranslate = Subtitles.Where(s =>
            s.TimeInterval.StartTime >= newSettings.FromTime && s.TimeInterval.EndTime <= newSettings.ToTime
        );

        var subtitlesDtos = _subtitlesMapper.VisualSubtitlesToSubtitleDtoList(subtitlesToTranslate);

        var results = _translationService.TranslateAsync(subtitlesDtos, newSettings.TargetLanguage, default);

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                await _builtInDialogService.DisplayError(result.Error);

                return;
            }

            Translations.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(result.Value));
        }
    }

    [RelayCommand]
    public void SubsScrolled()
    {
        SubtitlesCollectionState.AutoScrollEnabled = IsCurrentSubVisible;
    }

    [RelayCommand]
    public void TranslationsScrolled()
    {
        TranslationsCollectionState.AutoScrollEnabled = IsCurrentTranslationVisible;
    }
    #endregion

    #region private methods
    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("open", out object? value))
        {
            MediaPath = value.ToString();
        }
    }

    private static bool UpdateCurrentSubtitleIndex(
        TimeSpan currPosition,
        ObservableCollection<VisualSubtitle> subtitles,
        SubtitlesCollectionState collectionState
    )
    {
        if (subtitles is null or { Count: 0 })
        {
            return false;
        }

        var currIndex = collectionState.CurrentSubtitleIndex;
        var currSub = subtitles[currIndex];

        if (currSub.TimeInterval.ContainsTime(currPosition))
        {
            currSub.IsHighlighted = true;

            return false;
        }

        VisualSubtitle? prevSub = currIndex > 0 ? subtitles[currIndex - 1] : null;
        VisualSubtitle? nextSub = currIndex < subtitles.Count - 1 ? subtitles[currIndex + 1] : null;

        VisualSubtitle? newSub;
        int newIndex;

        if (prevSub is not null && prevSub.TimeInterval.ContainsTime(currPosition))
        {
            newSub = prevSub;
            newIndex = currIndex - 1;
        }
        else if (nextSub is not null && nextSub.TimeInterval.ContainsTime(currPosition))
        {
            newSub = nextSub;
            newIndex = currIndex + 1;
        }
        else
        {
            (newSub, newIndex) = subtitles.BinarySearch(currPosition);
        }

        if (newSub != null)
        {
            currSub.IsHighlighted = false;
            collectionState.CurrentSubtitleIndex = newIndex;
            newSub.IsHighlighted = true;

            return true;
        }

        return false;
    }

    #endregion
}
