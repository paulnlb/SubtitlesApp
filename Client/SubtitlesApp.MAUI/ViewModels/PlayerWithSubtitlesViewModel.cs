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
            TranslatedSubtitles.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(sub));
        }
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
