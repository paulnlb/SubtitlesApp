using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using SubtitlesApp.Mapper;
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
    private string? _mediaPath;

    [ObservableProperty]
    private int _transcribeBufferLength;

    [ObservableProperty]
    private SubtitlesSettings _subtitlesSettings;

    [ObservableProperty]
    private SubtitlesCollectionState _subtitlesCollectionState;

    [ObservableProperty]
    private TimeSpan _playerPosition = TimeSpan.Zero;

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
    private readonly TaskQueue _translationTaskQueue;
    private Task<ObservableCollectionResult<VisualSubtitle>>? _transcriptionTask;
    private CancellationTokenSource _transcriptionCts;
    private TranscriptionStatus _transcriptionStatus;
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
        SubtitlesMapper subtitlesMapper,
        ITranscriptionService transcriptionService,
        IBuiltInDialogService builtInDialogService
    )
    {
        #region observable properties

        PlayerControlsVisible = true;
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
            TranslationStreamingEnabled = true,
            AutoTranslationEnabled = true,
        };
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
        _translationTaskQueue = new TaskQueue();
        _transcriptionStatus = TranscriptionStatus.Ready;
        _builtInDialogService = builtInDialogService;
        #endregion
    }

    #region commands

    [RelayCommand]
    public void PositionChanged()
    {
        var currentPosition = PlayerPosition;
        UpdateCurrentSubtitleIndex(currentPosition);

        if (_transcriptionStatus == TranscriptionStatus.Ready && ShouldStartTranscription(currentPosition))
        {
            StartTranscription(currentPosition);
        }
    }

    [RelayCommand]
    public void Translate()
    {
        (var skippedSubsNumber, var subtitlesToTranslate) = FilterSubtitlesByCurrentScope();

        Subtitles.RestoreOriginalLanguages(skippedSubsNumber);

        _translationTaskQueue.EnqueueTask(async cancellationToken =>
        {
            SubtitlesCollectionState.IsTranslationRunning = true;

            var translationResult = await TranslateAsync(subtitlesToTranslate, cancellationToken);

            if (translationResult.IsFailure)
            {
                await MainThread.InvokeOnMainThreadAsync(() => _builtInDialogService.DisplayError(translationResult.Error));
            }

            SubtitlesCollectionState.IsTranslationRunning = false;
        });
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
        PlayerPosition = subtitle.TimeInterval.StartTime;
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

    #endregion

    #region public methods

    public void Clean()
    {
        _transcriptionCts?.Cancel();
        _translationTaskQueue.CancelAllTasks();
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

    private (int SkippedSubsNumber, IEnumerable<VisualSubtitle> FilteredSubs) FilterSubtitlesByCurrentScope()
    {
        var skippedSubsNumber = SubtitlesSettings.WhichSubtitlesToTranslate switch
        {
            SubtitlesCaptureMode.All => 0,
            SubtitlesCaptureMode.VisibleAndNext => SubtitlesCollectionState.FirstVisibleSubtitleIndex,
            SubtitlesCaptureMode.OnlyNext => SubtitlesCollectionState.LastVisibleSubtitleIndex + 1,
            _ => throw new NotImplementedException(),
        };

        var subtitlesToTranslate = Subtitles.Skip(skippedSubsNumber);
        return (skippedSubsNumber, subtitlesToTranslate);
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

    private bool ShouldStartTranscription(TimeSpan position)
    {
        return _subtitlesTimeSetService.ShouldStartTranscription(_coveredTimeIntervals, position, MediaDuration);
    }

    private void StartTranscription(TimeSpan currentPosition)
    {
        // We do not wait for a transcription task, but we store it the vm to track its progress.
        // Currently the business logic does not allow us to create a queue of transcription tasks
        // (like we do with translations)
        _transcriptionCts = new CancellationTokenSource();
        _transcriptionTask = TranscribeAsync(currentPosition, _transcriptionCts.Token);

        _transcriptionStatus = TranscriptionStatus.Transcribing;
        IsBusy = true;

        _transcriptionTask.ContinueWith(async a =>
        {
            ObservableCollectionResult<VisualSubtitle> transcriptionResult = a.Result;
            IsBusy = false;

            if (transcriptionResult.IsFailure && transcriptionResult.Error.Code != ErrorCode.OperationCanceled)
            {
                _transcriptionStatus = TranscriptionStatus.Error;
                await MainThread.InvokeOnMainThreadAsync(
                    () => _builtInDialogService.DisplayError(transcriptionResult.Error)
                );
                return;
            }

            _transcriptionStatus = TranscriptionStatus.Ready;

            if (SubtitlesSettings.AutoTranslationEnabled && SubtitlesSettings.TranslateToLanguage?.Code != null)
            {
                // As translation process may go slower than transcription, so we maintain
                // a queue of translation tasks to ensure they will all be executed
                // one by one, in FIFO manner
                _translationTaskQueue.EnqueueTask(async cancellationToken =>
                {
                    SubtitlesCollectionState.IsTranslationRunning = true;

                    var translationResult = await TranslateAsync(transcriptionResult.Value, cancellationToken);

                    if (translationResult.IsFailure && transcriptionResult.Error.Code != ErrorCode.OperationCanceled)
                    {
                        await MainThread.InvokeOnMainThreadAsync(
                            () => _builtInDialogService.DisplayError(translationResult.Error)
                        );
                    }

                    SubtitlesCollectionState.IsTranslationRunning = false;
                });
            }
        });
    }

    private async Task<ObservableCollectionResult<VisualSubtitle>> TranscribeAsync(
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

        var transcriptionResult = await _transcriptionService.TranscribeAsync(
            MediaPath,
            timeIntervalToTranscribe,
            SubtitlesSettings,
            cancellationToken
        );

        if (transcriptionResult.IsFailure)
        {
            return ObservableCollectionResult<VisualSubtitle>.Failure(transcriptionResult.Error);
        }

        var visualSubs = _subtitlesMapper.SubtitlesDtosToObservableVisualSubtitles(transcriptionResult.Value);

        InsertSubtitlesAndCoveredTime(visualSubs, timeIntervalToTranscribe);

        return ObservableCollectionResult<VisualSubtitle>.Success(visualSubs);
    }

    private async Task<Result> TranslateAsync(
        IEnumerable<VisualSubtitle> subtitlesToTranslate,
        CancellationToken cancellationToken = default
    )
    {
        if (!subtitlesToTranslate.Any())
        {
            return Result.Success();
        }

        var subtitlesDtos = _subtitlesMapper.VisualSubtitlesToSubtitleDtoList(subtitlesToTranslate);

        if (SubtitlesSettings.TranslationStreamingEnabled)
        {
            var translationResult = await _translationService.TranslateAndStreamAsync(
                subtitlesDtos,
                SubtitlesSettings.TranslateToLanguage!.Code,
                cancellationToken
            );

            if (translationResult.IsFailure)
            {
                return Result.Failure(translationResult.Error);
            }

            await UpdateSubtitlesTranslationsAsync(translationResult.Value, subtitlesToTranslate);
        }
        else
        {
            var translationResult = await _translationService.TranslateAsync(
                subtitlesDtos,
                SubtitlesSettings.TranslateToLanguage!.Code,
                cancellationToken
            );

            if (translationResult.IsFailure)
            {
                return Result.Failure(translationResult.Error);
            }

            UpdateSubtitlesTranslations(translationResult.Value, subtitlesToTranslate);
        }

        return Result.Success();
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

    private void UpdateSubtitleTranslation(SubtitleDto translationDto, VisualSubtitle subtitleToTranslate)
    {
        subtitleToTranslate.RestoreOriginalLanguage();
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

    private void UpdateSubtitlesTranslations(
        List<SubtitleDto> subtitleTranslationDtos,
        IEnumerable<VisualSubtitle> visualSubtitles
    )
    {
        foreach (var (translationDto, subtitleToTranslate) in subtitleTranslationDtos.Zip(visualSubtitles, Tuple.Create))
        {
            UpdateSubtitleTranslation(translationDto, subtitleToTranslate);
        }
    }

    private async Task UpdateSubtitlesTranslationsAsync(
        IAsyncEnumerable<SubtitleDto> subtitleTranslationDtos,
        IEnumerable<VisualSubtitle> visualSubtitles
    )
    {
        var translationEnumerator = subtitleTranslationDtos.GetAsyncEnumerator();
        foreach (var visualSubtitle in visualSubtitles)
        {
            if (!await translationEnumerator.MoveNextAsync())
            {
                break;
            }

            UpdateSubtitleTranslation(translationEnumerator.Current, visualSubtitle);
        }
    }

    #endregion

    #region event handlers

    partial void OnSubtitlesSettingsChanged(SubtitlesSettings? oldValue, SubtitlesSettings newValue)
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

        // Background translation switch is toggled
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

    #endregion
}
