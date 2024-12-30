﻿using AutoMapper;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiPageFullScreen;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Extensions;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
namespace SubtitlesApp.ViewModels;

public partial class PlayerWithSubtitlesViewModel : ObservableObject, IQueryAttributable
{
    #region observable properties

    [ObservableProperty]
    ObservableCollection<VisualSubtitle> _subtitles;

    [ObservableProperty]
    string _textBoxContent;

    [ObservableProperty]
    string? _mediaPath;

    [ObservableProperty]
    int _transcribeBufferLength;

    [ObservableProperty]
    TranscribeStatus _transcribeStatus = TranscribeStatus.Ready;

    [ObservableProperty]
    SubtitlesSettings _subtitlesSettings;

    [ObservableProperty]
    SubtitlesCollectionState _subtitlesCollectionState;

    [ObservableProperty]
    TimeSpan _positionToSeek = TimeSpan.Zero;

    [ObservableProperty]
    bool _playerControlsVisible;

    [ObservableProperty]
    bool _isSideChildVisible = true;

    [ObservableProperty]
    double _playerRelativeVerticalLength;

    [ObservableProperty]
    double _playerRelativeHorizontalLength;

    #endregion

    readonly ITranslationService _translationService;
    readonly IPopupService _popupService;
    readonly ISubtitlesTimeSetService _subtitlesTimeSetService;
    readonly IMapper _mapper;
    readonly ITranscriptionService _transcriptionService;
    readonly TimeSet _coveredTimeIntervals;

    public PlayerWithSubtitlesViewModel(
        ISettingsService settings,
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService,
        ISubtitlesTimeSetService subtitlesTimeSetService,
        IMapper mapper,
        ITranscriptionService transcriptionService)
    {
        #region observable props

        PlayerControlsVisible = true;
        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        TranscribeBufferLength = settings.TranscribeBufferLength;
        SubtitlesCollectionState = new SubtitlesCollectionState
        {
            AutoScrollEnabled = true,
        };
        SubtitlesSettings = new SubtitlesSettings
        {
            AvailableLanguages = languageService.GetAllLanguages(),
            OriginalLanguage = languageService.GetDefaultLanguage(),
            TranslateToLanguage = null,
            ShowTranslation = false,
            WhichSubtitlesToTranslate = SubtitlesCaptureMode.VisibleAndNext,
        };
        PlayerRelativeVerticalLength = 0.3;
        PlayerRelativeHorizontalLength = 0.65;
        
        #endregion

        #region private props

        _translationService = translationService;
        _popupService = popupService;
        _subtitlesTimeSetService = subtitlesTimeSetService;
        _mapper = mapper;
        _transcriptionService = transcriptionService;
        _coveredTimeIntervals = new TimeSet();

        #endregion

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
    }

    public ICommand TriggerResizeAnimationCommand { get; set; }

    public TimeSpan MediaDuration { get; set; }

    #region commands

    [RelayCommand]
    public void PositionChanged(TimeSpan currentPosition)
    {
        if (TranscribeStatus == TranscribeStatus.Ready
            && ShouldStartTranscription(currentPosition))
        {
            TranscribeFromPositionCommand.ExecuteAsync(currentPosition);
        }

        UpdateCurrentSubtitleIndex(currentPosition);
    }

    [RelayCommand]
    public async Task TranscribeFromPositionAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        var task = TranscribeAsync(position, cancellationToken);

        TextBoxContent = "Transcribing...";
        TranscribeStatus = TranscribeStatus.Transcribing;

        var result = await task;

        if (result.IsSuccess)
        {
            TextBoxContent = "Transcribing done.";
            TranscribeStatus = TranscribeStatus.Ready;
        }
        else
        {
            TextBoxContent = result.Error.Description;
            TranscribeStatus = TranscribeStatus.Error;
        }
    }

    #region subtitles scrolling
    [RelayCommand]
    public void SubtitlesScrolled()
    {
        if (SubtitlesCollectionState.CurrentSubtitleIndex > SubtitlesCollectionState.LastVisibleSubtitleIndex ||
            SubtitlesCollectionState.CurrentSubtitleIndex < SubtitlesCollectionState.FirstVisibleSubtitleIndex)
        {
            SubtitlesCollectionState.AutoScrollEnabled = false;
        }
        else
        {
            SubtitlesCollectionState.AutoScrollEnabled = true;
        }
    }

    [RelayCommand]
    public void ScrollToCurrentSub()
    {
        SubtitlesCollectionState.AutoScrollEnabled = true;

        SubtitlesCollectionState.ScrollToSubtitleIndex = SubtitlesCollectionState.CurrentSubtitleIndex;
    }
    #endregion

    [RelayCommand]
    public void SubtitleTapped(VisualSubtitle subtitle)
    {
        PositionToSeek = subtitle.TimeInterval.StartTime;
    }

    [RelayCommand]
    public async Task OpenSubtitlesSettings()
    {
        var result = await _popupService.ShowPopupAsync<SubtitlesSettingsPopupViewModel>(vm => vm.Settings = SubtitlesSettings.ShallowCopy());

        if (result is SubtitlesSettings newSettings)
        {
            SubtitlesSettings = newSettings;
        }
    }

    [RelayCommand]
    public void TogglePlayerControlsVisibility()
    {
        PlayerControlsVisible = !PlayerControlsVisible;
    }

    #region player swipe commands
    [RelayCommand]
    public void PlayerSwipedLeft()
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait && !IsSideChildVisible)
        {
            IsSideChildVisible = true;
        }
    }

    [RelayCommand]
    public void PlayerSwipedRight()
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait && IsSideChildVisible)
        {
            IsSideChildVisible = false;
        }
    }

    [RelayCommand]
    public void PlayerSwipedUp()
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait && !IsSideChildVisible)
        {
            IsSideChildVisible = true;
        }
    }

    [RelayCommand]
    public void PlayerSwipedDown()
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait && IsSideChildVisible)
        {
            IsSideChildVisible = false;
        }
    }
    #endregion

    #endregion

    #region public methods

    public async Task<Result> TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        var timeIntervalToTranscribe = _subtitlesTimeSetService.GetTimeIntervalForTranscription(
                _coveredTimeIntervals,
                position,
                TimeSpan.FromSeconds(TranscribeBufferLength),
                MediaDuration);

        if (timeIntervalToTranscribe == null)
        {
            return Result.Success();
        }

        var transcriptionResult = await _transcriptionService.TranscribeAsync(
            MediaPath,
            timeIntervalToTranscribe,
            SubtitlesSettings,
            cancellationToken);

        if (transcriptionResult.IsFailure)
        {
            return transcriptionResult;
        }

        InsertSubtitlesAndCoveredTime(transcriptionResult.Value, timeIntervalToTranscribe);

        return Result.Success();
    }

    public async Task TranslateExistingAsync()
    {
        if (SubtitlesSettings.TranslateToLanguage?.Code == null)
        {
            return;
        }

        var (skippedSubsNumber, subsToTranslate) = FilterSubtitlesByCurrentScope();

        if (!subsToTranslate.Any())
        {
            return;
        }

        var subtitlesDtos = _mapper.Map<List<SubtitleDTO>>(subsToTranslate);

        var translationResult = await _translationService.TranslateAsync(
            subtitlesDtos,
            SubtitlesSettings.TranslateToLanguage.Code);

        if (translationResult.IsFailure)
        {
            return;
        }

        UpdateSubtitles(translationResult.Value, skippedSubsNumber);
    }

    public void Clean()
    {
        TranscribeFromPositionCommand.Cancel();
        _transcriptionService.Dispose();
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
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

    VisualSubtitle? GetCurrentSubtitle()
    {
        if (Subtitles == null || Subtitles.Count == 0)
        {
            return null;
        }

        return Subtitles[SubtitlesCollectionState.CurrentSubtitleIndex];
    }

    bool ShouldStartTranscription(TimeSpan position)
    {
        return _subtitlesTimeSetService.ShouldStartTranscription(
            _coveredTimeIntervals,
            position,
            MediaDuration);
    }

    void UpdateCurrentSubtitleIndex(TimeSpan currentPosition)
    {
        var currentSubtitle = GetCurrentSubtitle();

        if (currentSubtitle == null)
        {
            return;
        }

        if (currentSubtitle.TimeInterval.ContainsTime(currentPosition))
        {
            currentSubtitle.IsHighlighted = true;
            return;
        }

        var (newSub, newIndex) = Subtitles.BinarySearch(currentPosition);

        if (newSub != null)
        {
            currentSubtitle.IsHighlighted = false;
            SubtitlesCollectionState.CurrentSubtitleIndex = newIndex;
            newSub.IsHighlighted = true;

            if (SubtitlesCollectionState.AutoScrollEnabled &&
                (newIndex <= SubtitlesCollectionState.FirstVisibleSubtitleIndex ||
                newIndex >= SubtitlesCollectionState.LastVisibleSubtitleIndex))
            {
                SubtitlesCollectionState.ScrollToSubtitleIndex = newIndex;
            }
        }
    }

    (int SkippedSubsNumber, IEnumerable<VisualSubtitle> FilteredSubs) FilterSubtitlesByCurrentScope()
    {
        var skippedSubsNumber = SubtitlesSettings.WhichSubtitlesToTranslate switch
        {
            SubtitlesCaptureMode.All => 0,
            SubtitlesCaptureMode.VisibleAndNext => SubtitlesCollectionState.FirstVisibleSubtitleIndex,
            SubtitlesCaptureMode.OnlyNext => SubtitlesCollectionState.LastVisibleSubtitleIndex + 1,
            _ => throw new NotImplementedException()
        };

        var subtitlesToTranslate = Subtitles.Skip(skippedSubsNumber);
        return (skippedSubsNumber, subtitlesToTranslate);
    }

    void InsertSubtitlesAndCoveredTime(List<SubtitleDTO> subtitles, TimeInterval timeIntervalToTranscribe)
    {
        var visualSubs = _mapper.Map<ObservableCollection<VisualSubtitle>>(subtitles);

        if (SubtitlesSettings.ShowTranslation)
        {
            Subtitles.InsertMany(visualSubs, x => x.SwitchToTranslation());
        }
        else
        {
            Subtitles.InsertMany(visualSubs);
        }

        var lastAddedSub = subtitles.LastOrDefault();

        if (lastAddedSub == null || timeIntervalToTranscribe.EndTime == MediaDuration)
        {
            _coveredTimeIntervals.Insert(
                new TimeInterval(timeIntervalToTranscribe));
        }
        else
        {
            _coveredTimeIntervals.Insert(
                new TimeInterval(
                    timeIntervalToTranscribe.StartTime,
                    lastAddedSub.TimeInterval.StartTime));
        }
    }

    void UpdateSubtitles(List<SubtitleDTO> subtitles, int skip)
    {
        var translatedSubs = _mapper.Map<ObservableCollection<VisualSubtitle>>(subtitles);

        if (SubtitlesSettings.ShowTranslation)
        {
            Subtitles.ReplaceMany(
                translatedSubs,
                x => x.SwitchToTranslation(),
                skip);
        }
        else
        {
            Subtitles.ReplaceMany(translatedSubs, skip);
        }
    }

    void ManageFullScreenMode(bool isSubtitlesVisible)
    {
        // Case 1: if subtitles are hidden, enter fullscreen
        if (!isSubtitlesVisible)
        {
            Controls.FullScreen();
        }

        // Case 2: if subtitles are visible and device is in portrait mode, exit fullscreen
        else if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            Controls.RestoreScreen();
        }
        // Case 3: if subtitles are visible and device is NOT in portrait mode, enter fullscreen
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            Controls.FullScreen();
        }
    }

    #endregion

    #region event handlers

    async partial void OnSubtitlesSettingsChanged(SubtitlesSettings? oldValue, SubtitlesSettings newValue)
    {
        if (oldValue == null)
        {
            // Short-circuit initial setup
            return;
        }

        // Priority 1: Language is changed
        // OR
        // Priority 2: Translation scope increased
        if (newValue.TranslateToLanguage != oldValue.TranslateToLanguage ||
            newValue.WhichSubtitlesToTranslate < oldValue.WhichSubtitlesToTranslate)
        {
            await TranslateExistingAsync();
        }

        // Priority 3: Background translation switch is toggled
        if (newValue.ShowTranslation != oldValue.ShowTranslation)
        {
            var (skippedSubsNumber, _) = FilterSubtitlesByCurrentScope();

            if (newValue.ShowTranslation)
            {
                Subtitles.SwitchToTranslations(skippedSubsNumber);
            }
            else
            {
                Subtitles.RestoreOriginalLanguages(skippedSubsNumber);
            }
        }
    }

    partial void OnIsSideChildVisibleChanged(bool value)
    {
        if (value)
        {
            PlayerRelativeHorizontalLength = 0.65;
            PlayerRelativeVerticalLength = 0.3;
        }
        else
        {
            PlayerRelativeHorizontalLength = PlayerRelativeVerticalLength = 1;
        }

        TriggerResizeAnimationCommand.Execute(CancellationToken.None);

        ManageFullScreenMode(value);
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        ManageFullScreenMode(IsSideChildVisible);
    }

    #endregion
}