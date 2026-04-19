using System.Collections.ObjectModel;
using Android.Net.IpSec.Ike;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Adapters;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.DTOs;
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
    private ObservableCollection<VisualSubtitle> _translatedSubtitles;

    [ObservableProperty]
    private ObservableCollectionAdapter<VisualSubtitle> _translatedSubtitlesAdapter;

    [ObservableProperty]
    private string? _mediaPath;

    [ObservableProperty]
    private SubtitlesCollectionState _subtitlesCollectionState;

    [ObservableProperty]
    private SubtitlesCollectionState _translatedSubtitlesCollectionState;

    [ObservableProperty]
    private bool _playerControlsVisible;

    [ObservableProperty]
    private PlayerSubtitlesLayoutSettings _layoutSettings;

    [ObservableProperty]
    private bool _isBusy;

    #endregion

    #region private fields
    private readonly ITranslationService _translationService;
    private readonly IPopupService _popupService;
    private readonly ISubtitlesTimeSetService _subtitlesTimeSetService;
    private readonly SubtitlesMapper _subtitlesMapper;
    private readonly ITranscriptionService _transcriptionService;
    private readonly IBuiltInDialogService _builtInDialogService;

    private readonly TimeSet _coveredTimeIntervals;
    private Language _subtitlesLanguage;
    #endregion

    public TimeSpan MediaDuration { get; set; }

    public PlayerWithSubtitlesViewModel(
        ISettingsService settings,
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService,
        ISubtitlesTimeSetService subtitlesTimeSetService,
        SubtitlesMapper subtitlesMapper,
        ITranscriptionService transcriptionService,
        IBuiltInDialogService builtInDialogService
    )
    {
        #region observable properties

        PlayerControlsVisible = true;
        MediaPath = null;
        Subtitles = [];
        TranslatedSubtitles = [];
        SubtitlesAdapter = new ObservableCollectionAdapter<VisualSubtitle>(_subtitles);
        TranslatedSubtitlesAdapter = new ObservableCollectionAdapter<VisualSubtitle>(_translatedSubtitles);
        SubtitlesCollectionState = new SubtitlesCollectionState { AutoScrollEnabled = true };
        TranslatedSubtitlesCollectionState = new SubtitlesCollectionState { AutoScrollEnabled = true };
        LayoutSettings = new PlayerSubtitlesLayoutSettings
        {
            PlayerRelativeVerticalLength = 0.3,
            PlayerRelativeHorizontalLength = 0.65,
            SubtitlesRelativeVerticalLength = 0.7,
            SubtitlesRelativeHorizontalLength = 0.35,
        };

        #endregion

        #region private properties
        _translationService = translationService;
        _popupService = popupService;
        _subtitlesTimeSetService = subtitlesTimeSetService;
        _subtitlesMapper = subtitlesMapper;
        _transcriptionService = transcriptionService;
        _coveredTimeIntervals = new TimeSet();
        _builtInDialogService = builtInDialogService;
        _subtitlesLanguage = languageService.GetDefaultLanguage();
        #endregion
    }

    #region commands

    [RelayCommand]
    public void PositionChanged(TimeSpan currentPosition)
    {
        UpdateCurrentSubtitleIndex(currentPosition);
    }

    #region subtitles scrolling
    [RelayCommand]
    public void SubtitlesScrolled()
    {
        if (
            SubtitlesCollectionState.CurrentSubtitleIndex > SubtitlesCollectionState.LastVisibleSubtitleIndex
            || SubtitlesCollectionState.CurrentSubtitleIndex < SubtitlesCollectionState.FirstVisibleSubtitleIndex
        )
        {
            SubtitlesCollectionState.AutoScrollEnabled = false;
        }
        else
        {
            SubtitlesCollectionState.AutoScrollEnabled = true;
        }
    }

    [RelayCommand]
    public void EnableAutoScroll()
    {
        SubtitlesCollectionState.AutoScrollEnabled = true;
    }
    #endregion

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
    public async Task OpenTranscribePopup()
    {
        var result = await _popupService.ShowPopupAsync<TranscribePopupViewModel>(vm =>
        {
            vm.MediaDuration = MediaDuration;
            vm.SubtitlesLanguage = _subtitlesLanguage;
        });

        if (result is not TranscriptionSettings transcriptionSettings)
        {
            return;
        }

        _subtitlesLanguage = transcriptionSettings.SubtitlesLanguage;
        var timeIntervalToTranscribe = new TimeInterval(transcriptionSettings.FromTime, transcriptionSettings.ToTime);

        var transcriptionResult = await _transcriptionService.TranscribeAsync(
            MediaPath,
            timeIntervalToTranscribe,
            _subtitlesLanguage.Code,
            default
        );

        if (transcriptionResult.IsFailure)
        {
            await _builtInDialogService.DisplayError(transcriptionResult.Error);

            return;
        }

        var visualSubs = _subtitlesMapper.SubtitlesDtosToObservableVisualSubtitles(transcriptionResult.Value);

        InsertSubtitlesAndCoveredTime(visualSubs, timeIntervalToTranscribe);
    }

    [RelayCommand]
    public async Task OpenTranslatePopup()
    {
        var result = await _popupService.ShowPopupAsync<TranslatePopupViewModel>(vm =>
        {
            vm.MediaDuration = MediaDuration;
            vm.SourceLanguageCode = _subtitlesLanguage.Code;
        });

        if (result is not TranslationSettings translationSettings)
        {
            return;
        }

        var subtitlesToTranslate = Subtitles.Where(s =>
            s.TimeInterval.StartTime >= translationSettings.FromTime && s.TimeInterval.EndTime <= translationSettings.ToTime
        );

        var subtitlesDtos = _subtitlesMapper.VisualSubtitlesToSubtitleDtoList(subtitlesToTranslate);

        var translationResult = await _translationService.TranslateAndStreamAsync(
            subtitlesDtos,
            translationSettings.TargetLanguage.Code,
            default
        );

        if (translationResult.IsFailure)
        {
            await _builtInDialogService.DisplayError(translationResult.Error);

            return;
        }

        await InsertTranslatedSubtitles(translationResult.Value);
    }

    #endregion

    #region public methods

    public void Clean()
    {
        _transcriptionService.Dispose();
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

    private VisualSubtitle? GetCurrentSubtitle()
    {
        if (Subtitles == null || Subtitles.Count == 0)
        {
            return null;
        }

        return Subtitles[SubtitlesCollectionState.CurrentSubtitleIndex];
    }

    private void InsertSubtitlesAndCoveredTime(
        ObservableCollection<VisualSubtitle> subtitles,
        TimeInterval timeIntervalToTranscribe
    )
    {
        var lastAddedSub = subtitles.LastOrDefault();

        if (lastAddedSub == null || timeIntervalToTranscribe.EndTime == MediaDuration)
        {
            _coveredTimeIntervals.Insert(new TimeInterval(timeIntervalToTranscribe));
        }
        else
        {
            _coveredTimeIntervals.Insert(
                new TimeInterval(timeIntervalToTranscribe.StartTime, lastAddedSub.TimeInterval.StartTime)
            );
        }

        Subtitles.InsertMany(subtitles);
    }

    private async Task InsertTranslatedSubtitles(IAsyncEnumerable<SubtitleDto> translatedSubtitles)
    {
        await foreach (var sub in translatedSubtitles)
        {
            TranslatedSubtitles.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(sub));
        }
    }

    private void UpdateCurrentSubtitleIndex(TimeSpan currentPosition)
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
        }
    }

    #endregion
}
