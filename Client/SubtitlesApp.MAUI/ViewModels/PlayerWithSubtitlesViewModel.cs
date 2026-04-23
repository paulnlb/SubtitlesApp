using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Adapters;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Extensions;
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
    private PlayerSubtitlesLayoutSettings _layoutSettings;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isSubtitlesSelected;

    [ObservableProperty]
    private bool _isTranslationsSelected;

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

    public event EventHandler? SubsScrollRequested;
    public event EventHandler? TranslationsScrollRequested;

    public TimeSpan MediaDuration { get; set; }

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
        LayoutSettings = new PlayerSubtitlesLayoutSettings
        {
            PlayerRelativeVerticalLength = 0.3,
            PlayerRelativeHorizontalLength = 0.65,
            SubtitlesRelativeVerticalLength = 0.7,
            SubtitlesRelativeHorizontalLength = 0.35,
        };

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
        if (IsSubtitlesSelected)
        {
            var isUpdated = UpdateCurrentSubtitleIndex(currentPosition, Subtitles, SubtitlesCollectionState);

            if (isUpdated && SubtitlesCollectionState.AutoScrollEnabled)
            {
                SubsScrollRequested?.Invoke(this, EventArgs.Empty);
            }

            UpdateCurrentSubtitleIndex(currentPosition, Translations, TranslationsCollectionState);
        }
        else if (IsTranslationsSelected)
        {
            var isUpdated = UpdateCurrentSubtitleIndex(currentPosition, Translations, TranslationsCollectionState);

            if (isUpdated && TranslationsCollectionState.AutoScrollEnabled)
            {
                TranslationsScrollRequested?.Invoke(this, EventArgs.Empty);
            }

            UpdateCurrentSubtitleIndex(currentPosition, Subtitles, SubtitlesCollectionState);
        }
    }

    [RelayCommand]
    public void ScrollToFocused()
    {
        if (IsSubtitlesSelected)
        {
            SubsScrollRequested?.Invoke(this, EventArgs.Empty);
            SubtitlesCollectionState.AutoScrollEnabled = true;
        }
        else
        {
            TranslationsScrollRequested?.Invoke(this, EventArgs.Empty);
            TranslationsCollectionState.AutoScrollEnabled = true;
        }
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
        object? result;

        if (_transcriptionSettings is null)
        {
            result = await _popupService.ShowPopupAsync<TranscribePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.SubtitlesLanguage = _languageService.GetDefaultLanguage();
            });
        }
        else
        {
            result = await _popupService.ShowPopupAsync<TranscribePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.SubtitlesLanguage = _transcriptionSettings.SubtitlesLanguage;
                vm.FromTime = _transcriptionSettings.FromTime;
                vm.ToTime = _transcriptionSettings.ToTime;
            });
        }

        if (result is not TranscriptionSettings newSettings)
        {
            return;
        }

        _transcriptionSettings = newSettings;

        var transcriptionResult = await _transcriptionService.TranscribeAsync(
            MediaPath,
            new TimeInterval(newSettings.FromTime, newSettings.ToTime),
            newSettings.SubtitlesLanguage.Code,
            default
        );

        if (transcriptionResult.IsFailure)
        {
            await _builtInDialogService.DisplayError(transcriptionResult.Error);

            return;
        }

        var visualSubs = _subtitlesMapper.SubtitlesDtosToObservableVisualSubtitles(transcriptionResult.Value);

        Subtitles.InsertMany(visualSubs);
    }

    [RelayCommand]
    public async Task Translate()
    {
        object? result;

        if (_translationSettings is null)
        {
            result = await _popupService.ShowPopupAsync<TranslatePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
            });
        }
        else
        {
            result = await _popupService.ShowPopupAsync<TranslatePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.TargetLanguage = _translationSettings.TargetLanguage;
                vm.FromTime = _translationSettings.FromTime;
                vm.ToTime = _translationSettings.ToTime;
            });
        }

        if (result is not TranslationSettings newSettings)
        {
            return;
        }

        _translationSettings = newSettings;

        var subtitlesToTranslate = Subtitles.Where(s =>
            s.TimeInterval.StartTime >= newSettings.FromTime && s.TimeInterval.EndTime <= newSettings.ToTime
        );

        var subtitlesDtos = _subtitlesMapper.VisualSubtitlesToSubtitleDtoList(subtitlesToTranslate);

        var translationResult = await _translationService.TranslateAndStreamAsync(
            subtitlesDtos,
            newSettings.TargetLanguage.Code,
            default
        );

        if (translationResult.IsFailure)
        {
            await _builtInDialogService.DisplayError(translationResult.Error);

            return;
        }

        await foreach (var sub in translationResult.Value)
        {
            Translations.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(sub));
        }
    }

    #endregion

    #region public methods

    public void Clean()
    {
        _transcriptionService.Dispose();
        SubsScrollRequested = null;
        TranslationsScrollRequested = null;
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
