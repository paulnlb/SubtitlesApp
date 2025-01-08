using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMapper;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiPageFullScreen;
using Microsoft.Maui.Adapters;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Extensions;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Core.Utils;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels;

public partial class PlayerWithSubtitlesViewModel : ObservableObject, IQueryAttributable
{
    #region observable properties

    [ObservableProperty]
    ObservableCollection<VisualSubtitle> _subtitles;

    [ObservableProperty]
    ObservableCollectionAdapter<VisualSubtitle> _subtitlesAdapter;

    [ObservableProperty]
    string _textBoxContent;

    [ObservableProperty]
    string? _mediaPath;

    [ObservableProperty]
    int _transcribeBufferLength;

    [ObservableProperty]
    SubtitlesSettings _subtitlesSettings;

    [ObservableProperty]
    SubtitlesCollectionState _subtitlesCollectionState;

    [ObservableProperty]
    TimeSpan _positionToSeek = TimeSpan.Zero;

    [ObservableProperty]
    bool _playerControlsVisible;

    [ObservableProperty]
    PlayerSubtitlesLayoutSettings _layoutSettings;

    #endregion

    #region private fields
    readonly ITranslationService _translationService;
    readonly IPopupService _popupService;
    readonly ISubtitlesTimeSetService _subtitlesTimeSetService;
    readonly IMapper _mapper;
    readonly ITranscriptionService _transcriptionService;

    readonly TimeSet _coveredTimeIntervals;
    readonly TaskQueue _translationTaskQueue;
    Task<ObservableCollectionResult<VisualSubtitle>>? _transcriptionTask;
    CancellationTokenSource _transcriptionCts;
    #endregion

    #region public properties
    public ICommand TriggerResizeAnimationCommand { get; set; }
    public TimeSpan MediaDuration { get; set; }
    #endregion

    public PlayerWithSubtitlesViewModel(
        ISettingsService settings,
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService,
        ISubtitlesTimeSetService subtitlesTimeSetService,
        IMapper mapper,
        ITranscriptionService transcriptionService
    )
    {
        #region observable properties

        PlayerControlsVisible = true;
        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        SubtitlesAdapter = new ObservableCollectionAdapter<VisualSubtitle>(_subtitles);
        TranscribeBufferLength = settings.TranscribeBufferLength;
        SubtitlesCollectionState = new SubtitlesCollectionState { AutoScrollEnabled = true };
        SubtitlesSettings = new SubtitlesSettings
        {
            AvailableLanguages = languageService.GetAllLanguages(),
            OriginalLanguage = languageService.GetDefaultLanguage(),
            TranslateToLanguage = null,
            ShowTranslation = false,
            WhichSubtitlesToTranslate = SubtitlesCaptureMode.VisibleAndNext,
        };
        LayoutSettings.PlayerRelativeVerticalLength = 0.3;
        LayoutSettings.PlayerRelativeHorizontalLength = 0.65;

        #endregion

        #region private properties
        _translationService = translationService;
        _popupService = popupService;
        _subtitlesTimeSetService = subtitlesTimeSetService;
        _mapper = mapper;
        _transcriptionService = transcriptionService;
        _coveredTimeIntervals = new TimeSet();
        _translationTaskQueue = new TaskQueue();
        #endregion

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        LayoutSettings.IsSideChildVisibleChanged += OnIsSideChildVisibleChanged;
    }

    #region commands

    [RelayCommand]
    public void PositionChanged(TimeSpan currentPosition)
    {
        UpdateCurrentSubtitleIndex(currentPosition);

        // We do not wait for a transcription task, but we store it the vm to track its progress.
        // Currently the business logic does not allow us to create a queue of transcription tasks
        // (like we do with translations). Instead, we check the previous transcription task's status.
        // If that task not running at the moment, we start a new one
        if (
            _transcriptionTask?.Status != TaskStatus.WaitingForActivation
            && ShouldStartTranscription(currentPosition)
        )
        {
            _transcriptionCts = new CancellationTokenSource();
            _transcriptionTask = TranscribeAsync(currentPosition, _transcriptionCts.Token);
            _transcriptionTask.ContinueWith(a =>
            {
                ObservableCollectionResult<VisualSubtitle> transcriptionResult = a.Result;

                if (transcriptionResult.IsFailure)
                {
                    return;
                }

                if (SubtitlesSettings.TranslateToLanguage?.Code != null)
                {
                    // As translation process may be slower than transcription, we maintain
                    // a queue of translation tasks to ensure they will all be executed
                    // one by one, in FIFO manner
                    _translationTaskQueue.EnqueueTask(cancellationToken =>
                        TranslateAndStreamAsync(transcriptionResult.Value, cancellationToken)
                    );
                }
            });
        }
    }

    #region subtitles scrolling
    [RelayCommand]
    public void SubtitlesScrolled()
    {
        if (
            SubtitlesCollectionState.CurrentSubtitleIndex
                > SubtitlesCollectionState.LastVisibleSubtitleIndex
            || SubtitlesCollectionState.CurrentSubtitleIndex
                < SubtitlesCollectionState.FirstVisibleSubtitleIndex
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
    public void ScrollToCurrentSub()
    {
        SubtitlesCollectionState.AutoScrollEnabled = true;

        SubtitlesCollectionState.ScrollToSubtitleIndex =
            SubtitlesCollectionState.CurrentSubtitleIndex;
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
        var result = await _popupService.ShowPopupAsync<SubtitlesSettingsPopupViewModel>(vm =>
            vm.Settings = SubtitlesSettings.ShallowCopy()
        );

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
        if (
            DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait
            && !LayoutSettings.IsSideChildVisible
        )
        {
            LayoutSettings.IsSideChildVisible = true;
        }
    }

    [RelayCommand]
    public void PlayerSwipedRight()
    {
        if (
            DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait
            && LayoutSettings.IsSideChildVisible
        )
        {
            LayoutSettings.IsSideChildVisible = false;
        }
    }

    [RelayCommand]
    public void PlayerSwipedUp()
    {
        if (
            DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait
            && !LayoutSettings.IsSideChildVisible
        )
        {
            LayoutSettings.IsSideChildVisible = true;
        }
    }

    [RelayCommand]
    public void PlayerSwipedDown()
    {
        if (
            DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait
            && LayoutSettings.IsSideChildVisible
        )
        {
            LayoutSettings.IsSideChildVisible = false;
        }
    }
    #endregion

    #endregion

    #region public methods

    public void Clean()
    {
        _transcriptionCts?.Cancel();
        _translationTaskQueue.CancelAllTasks();
        _transcriptionService.Dispose();
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
        LayoutSettings.IsSideChildVisibleChanged -= OnIsSideChildVisibleChanged;
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

    (
        int SkippedSubsNumber,
        IEnumerable<VisualSubtitle> FilteredSubs
    ) FilterSubtitlesByCurrentScope()
    {
        var skippedSubsNumber = SubtitlesSettings.WhichSubtitlesToTranslate switch
        {
            SubtitlesCaptureMode.All => 0,
            SubtitlesCaptureMode.VisibleAndNext =>
                SubtitlesCollectionState.FirstVisibleSubtitleIndex,
            SubtitlesCaptureMode.OnlyNext => SubtitlesCollectionState.LastVisibleSubtitleIndex + 1,
            _ => throw new NotImplementedException(),
        };

        var subtitlesToTranslate = Subtitles.Skip(skippedSubsNumber);
        return (skippedSubsNumber, subtitlesToTranslate);
    }

    VisualSubtitle? GetCurrentSubtitle()
    {
        if (Subtitles == null || Subtitles.Count == 0)
        {
            return null;
        }

        return Subtitles[SubtitlesCollectionState.CurrentSubtitleIndex];
    }

    void InsertSubtitlesAndCoveredTime(
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
                new TimeInterval(
                    timeIntervalToTranscribe.StartTime,
                    lastAddedSub.TimeInterval.StartTime
                )
            );

            subtitles.RemoveAt(subtitles.Count - 1);
        }

        Subtitles.InsertMany(subtitles);
    }

    bool ShouldStartTranscription(TimeSpan position)
    {
        return _subtitlesTimeSetService.ShouldStartTranscription(
            _coveredTimeIntervals,
            position,
            MediaDuration
        );
    }

    public async Task<ObservableCollectionResult<VisualSubtitle>> TranscribeAsync(
        TimeSpan position,
        CancellationToken cancellationToken = default
    )
    {
        var timeIntervalToTranscribe = _subtitlesTimeSetService.GetTimeIntervalForTranscription(
            _coveredTimeIntervals,
            position,
            TimeSpan.FromSeconds(TranscribeBufferLength),
            MediaDuration
        );

        if (timeIntervalToTranscribe == null)
        {
            return ObservableCollectionResult<VisualSubtitle>.Failure(
                new Error(ErrorCode.Unspecified, "Time interval to transcribe is empty.")
            );
        }

        TextBoxContent = "Transcribing...";

        var transcriptionResult = await _transcriptionService.TranscribeAsync(
            MediaPath,
            timeIntervalToTranscribe,
            SubtitlesSettings,
            cancellationToken
        );

        if (transcriptionResult.IsFailure)
        {
            TextBoxContent = transcriptionResult.Error.Description;
            return ObservableCollectionResult<VisualSubtitle>.Failure(transcriptionResult.Error);
        }

        var visualSubs = _mapper.Map<ObservableCollection<VisualSubtitle>>(
            transcriptionResult.Value
        );

        InsertSubtitlesAndCoveredTime(visualSubs, timeIntervalToTranscribe);

        TextBoxContent = "Transcribing done.";

        return ObservableCollectionResult<VisualSubtitle>.Success(visualSubs);
    }

    public async Task TranslateAsync(
        IEnumerable<VisualSubtitle> subtitlesToTranslate,
        CancellationToken cancellationToken = default
    )
    {
        if (!subtitlesToTranslate.Any())
        {
            return;
        }

        var subtitlesDtos = _mapper.Map<List<SubtitleDto>>(subtitlesToTranslate);

        TextBoxContent = "Translating...";

        var translationResult = await _translationService.TranslateAsync(
            subtitlesDtos,
            SubtitlesSettings.TranslateToLanguage!.Code,
            cancellationToken
        );

        if (translationResult.IsFailure)
        {
            TextBoxContent = translationResult.Error.Description;
            return;
        }

        UpdateSubtitlesTranslations(translationResult.Value, subtitlesToTranslate);

        TextBoxContent = "Translation completed.";
    }

    public async Task TranslateAndStreamAsync(
        IEnumerable<VisualSubtitle> subtitlesToTranslate,
        CancellationToken cancellationToken = default
    )
    {
        if (!subtitlesToTranslate.Any())
        {
            return;
        }

        var subtitlesDtos = _mapper.Map<List<SubtitleDto>>(subtitlesToTranslate);

        TextBoxContent = "Translating...";

        var translationResult = await _translationService.TranslateAndStreamAsync(
            subtitlesDtos,
            SubtitlesSettings.TranslateToLanguage!.Code,
            cancellationToken
        );

        if (translationResult.IsFailure)
        {
            TextBoxContent = translationResult.Error.Description;
            return;
        }

        await UpdateSubtitlesTranslations(translationResult.Value, subtitlesToTranslate);

        TextBoxContent = "Translation completed.";
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

            if (
                SubtitlesCollectionState.AutoScrollEnabled
                && (
                    newIndex <= SubtitlesCollectionState.FirstVisibleSubtitleIndex
                    || newIndex >= SubtitlesCollectionState.LastVisibleSubtitleIndex
                )
            )
            {
                SubtitlesCollectionState.ScrollToSubtitleIndex = newIndex;
            }
        }
    }

    void UpdateSubtitlesTranslations(
        List<SubtitleDto> subtitleTranslationDtos,
        IEnumerable<VisualSubtitle> visualSubtitles
    )
    {
        foreach (
            var (translationDto, subtitleToTranslate) in subtitleTranslationDtos.Zip(
                visualSubtitles,
                Tuple.Create
            )
        )
        {
            subtitleToTranslate.Translation = new Translation
            {
                LanguageCode = translationDto.LanguageCode,
                Text = translationDto.Text,
            };

            if (SubtitlesSettings.ShowTranslation)
            {
                subtitleToTranslate.SwitchToTranslation();
            }
        }
    }

    async Task UpdateSubtitlesTranslations(
        IAsyncEnumerable<SubtitleDto> subtitleTranslationDtos,
        IEnumerable<VisualSubtitle> visualSubtitles
    )
    {
        var visualSubtitlesEnumerator = visualSubtitles.GetEnumerator();
        await foreach (var subtitleTranslationDto in subtitleTranslationDtos)
        {
            if (!visualSubtitlesEnumerator.MoveNext())
            {
                break;
            }

            var translationDto = subtitleTranslationDto;
            var subtitleToTranslate = visualSubtitlesEnumerator.Current;

            subtitleToTranslate.Translation = new Translation
            {
                LanguageCode = translationDto.LanguageCode,
                Text = translationDto.Text,
            };

            if (SubtitlesSettings.ShowTranslation)
            {
                subtitleToTranslate.SwitchToTranslation();
            }
        }
    }

    static void ManageFullScreenMode(bool isSubtitlesVisible)
    {
        // Case 1: if subtitlesDtos are hidden, enter fullscreen
        if (!isSubtitlesVisible)
        {
            Controls.FullScreen();
        }
        // Case 2: if subtitlesDtos are visible and device is in portrait mode, exit fullscreen
        else if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            Controls.RestoreScreen();
        }
        // Case 3: if subtitlesDtos are visible and device is NOT in portrait mode, enter fullscreen
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            Controls.FullScreen();
        }
    }

    #endregion

    #region event handlers

    async partial void OnSubtitlesSettingsChanged(
        SubtitlesSettings? oldValue,
        SubtitlesSettings newValue
    )
    {
        // Skip SubtitlesSettings object initialization
        if (oldValue == null)
        {
            return;
        }

        // Skip if translation has been disabled
        if (newValue.TranslateToLanguage?.Code == null)
        {
            return;
        }

        // Priority 1: Language is changed
        // OR
        // Priority 2: Translation scope increased
        if (
            newValue.TranslateToLanguage != oldValue.TranslateToLanguage
            || newValue.WhichSubtitlesToTranslate < oldValue.WhichSubtitlesToTranslate
        )
        {
            (var skippedSubsNumber, var subtitlesToTranslate) = FilterSubtitlesByCurrentScope();

            Subtitles.RestoreOriginalLanguages(skippedSubsNumber);

            await TranslateAndStreamAsync(subtitlesToTranslate);
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

    private void OnIsSideChildVisibleChanged(bool value)
    {
        if (value)
        {
            LayoutSettings.PlayerRelativeHorizontalLength = 0.65;
            LayoutSettings.PlayerRelativeVerticalLength = 0.3;
        }
        else
        {
            LayoutSettings.PlayerRelativeHorizontalLength =
                LayoutSettings.PlayerRelativeVerticalLength = 1;
        }

        TriggerResizeAnimationCommand.Execute(CancellationToken.None);

        ManageFullScreenMode(value);
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        ManageFullScreenMode(LayoutSettings.IsSideChildVisible);
    }

    #endregion
}
