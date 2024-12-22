﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Extensions;
using System.Collections.ObjectModel;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using CommunityToolkit.Maui.Core;
using AutoMapper;
namespace SubtitlesApp.ViewModels;

public partial class MediaElementViewModel : ObservableObject, IQueryAttributable
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
    int _currentSubtitleIndex = 0;

    [ObservableProperty]
    int _scrollToSubtitleIndex = 0;

    [ObservableProperty]
    int _firstVisibleSubtitleIndex = 0;

    [ObservableProperty]
    int _lastVisibleSubtitleIndex = 0;

    [ObservableProperty]
    bool _autoScrollEnabled = true;

    [ObservableProperty]
    TimeSpan _positionToSeek = TimeSpan.Zero;
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISubtitlesService _subtitlesService;
    readonly ITranslationService _translationService;
    readonly IPopupService _popupService;
    readonly ISubtitlesTimeSetService _subtitlesTimeSetService;
    readonly IMapper _mapper;
    readonly TimeSet _coveredTimeIntervals;

    public MediaElementViewModel(
        IMediaProcessor mediaProcessor,
        ISettingsService settings,
        ISubtitlesService subtitlesService,
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService,
        ISubtitlesTimeSetService subtitlesTimeSetService,
        IMapper mapper)
    {
        #region observable props

        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        TranscribeBufferLength = settings.TranscribeBufferLength;
        SubtitlesSettings = new SubtitlesSettings
        {
            AvailableLanguages = languageService.GetAllLanguages(),
            OriginalLanguage = languageService.GetDefaultLanguage(),
            TranslateToLanguage = null,
            ShowTranslation = false,
            WhichSubtitlesToTranslate = SubtitlesCaptureMode.VisibleAndNext,
        };

        #endregion

        #region private props

        _mediaProcessor = mediaProcessor;
        _subtitlesService = subtitlesService;
        _translationService = translationService;
        _popupService = popupService;
        _subtitlesTimeSetService = subtitlesTimeSetService;
        _mapper = mapper;
        _coveredTimeIntervals = new TimeSet();

        #endregion
    }

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
    public void SeekCompleted(TimeSpan position)
    {
        if (TranscribeStatus == TranscribeStatus.Transcribing
            && ShouldStartTranscription(position))
        {
            TranscribeFromPositionCommand.Cancel();
        }
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

    [RelayCommand]
    public void SubtitlesScrolled()
    {
        if (ScrollToSubtitleIndex < FirstVisibleSubtitleIndex || ScrollToSubtitleIndex > LastVisibleSubtitleIndex)
        {
            AutoScrollEnabled = false;
        }
        else
        {
            AutoScrollEnabled = true;
        }
    }

    [RelayCommand]
    public void ScrollToCurrentSub()
    {
        AutoScrollEnabled = true;

        ScrollToSubtitleIndex = CurrentSubtitleIndex;
    }

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

    #endregion

    #region public methods

    VisualSubtitle? GetCurrentSubtitle()
    {
        if (Subtitles == null || Subtitles.Count == 0)
        {
            return null;
        }

        return Subtitles[CurrentSubtitleIndex];
    }

    // Todo: Separation of conserns between this VM and SubtitlesService
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

        try
        {
            var audio = await _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                timeIntervalToTranscribe.StartTime,
                timeIntervalToTranscribe.EndTime,
                cancellationToken);

            var subsResult = await _subtitlesService.GetSubsAsync(
                audio,
                SubtitlesSettings.OriginalLanguage.Code,
                timeIntervalToTranscribe.StartTime,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (subsResult.IsFailure)
            {
                return subsResult;
            }

            var subs = subsResult.Value;

            if (SubtitlesSettings.TranslateToLanguage?.Code != null)
            {
                var request = new TranslationRequestDto
                {
                    TargetLanguageCode = SubtitlesSettings.TranslateToLanguage.Code,
                    SourceSubtitles = subs,
                };

                var subsTranslationResult = await _translationService.TranslateAsync(request);

                if (subsTranslationResult.IsSuccess)
                {
                    subs = subsTranslationResult.Value;
                }
            }

            AddToObservables(subs);

            var lastAddedSub = subs.LastOrDefault();

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

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Success();
        }
    }

    public async Task TranslateAsync()
    {
        if (SubtitlesSettings.TranslateToLanguage?.Code == null)
        {
            return;
        }

        var startingIndex = SubtitlesSettings.WhichSubtitlesToTranslate switch
        {
            SubtitlesCaptureMode.All => 0,
            SubtitlesCaptureMode.VisibleAndNext => FirstVisibleSubtitleIndex,
            SubtitlesCaptureMode.OnlyNext => LastVisibleSubtitleIndex + 1,
            _ => throw new NotImplementedException()
        };

        var subtitlesToTranslate = Subtitles.Skip(startingIndex);

        if (!subtitlesToTranslate.Any())
        {
            return;
        }

        var subtitlesDtos = _mapper.Map<List<SubtitleDTO>>(subtitlesToTranslate);

        var request = new TranslationRequestDto
        {
            TargetLanguageCode = SubtitlesSettings.TranslateToLanguage.Code,
            SourceSubtitles = subtitlesDtos,
        };

        var subsTranslationResult = await _translationService.TranslateAsync(request);

        if (subsTranslationResult.IsSuccess)
        {
            AddToObservablesFromIndex(
                subsTranslationResult.Value, 
                startingIndex, 
                SubtitlesSettings.ShowTranslation);
        }
    }

    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("open", out object? value))
        {
            MediaPath = value.ToString();
        }
    }

    public void Clean()
    {
        TranscribeFromPositionCommand.Cancel();
        _mediaProcessor.Dispose();
    }
    #endregion

    #region private methods

    /// <summary>
    /// Adds subtitles to the observable list.
    /// </summary>
    /// <param name="subsToAdd"></param>
    void AddToObservables(
        IEnumerable<SubtitleDTO> subsToAdd)
    {
        foreach (var subtitleDto in subsToAdd)
        {
            var subtitle = _mapper.Map<VisualSubtitle>(subtitleDto);

            Subtitles.Insert(subtitle);
        }
    }

    void AddToObservablesFromIndex(
        IEnumerable<SubtitleDTO> subsToAdd,
        int insertIndex,
        bool showTranslation)
    {
        foreach (var subtitleDto in subsToAdd)
        {
            var subtitle = _mapper.Map<VisualSubtitle>(subtitleDto);

            if (showTranslation)
            {
                subtitle.SwitchToTranslation();
            }

            Subtitles[insertIndex] = subtitle;
            insertIndex++;
        }
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
            CurrentSubtitleIndex = newIndex;
            newSub.IsHighlighted = true;

            if (AutoScrollEnabled)
            {
                ScrollToSubtitleIndex = newIndex;
            }
        }
    }

    void ApplyTranslations()
    {
        var startingIndex = SubtitlesSettings.WhichSubtitlesToTranslate switch
        {
            SubtitlesCaptureMode.All => 0,
            SubtitlesCaptureMode.VisibleAndNext => FirstVisibleSubtitleIndex,
            SubtitlesCaptureMode.OnlyNext => LastVisibleSubtitleIndex + 1,
            _ => throw new NotImplementedException()
        };

        var subtitlesToTranslate = Subtitles.Skip(startingIndex);

        if (!subtitlesToTranslate.Any())
        {
            return;
        }

        foreach (var subtitle in subtitlesToTranslate)
        {
            if (!SubtitlesSettings.ShowTranslation && subtitle.IsTranslated)
            {
                subtitle.RestoreOriginalLanguage();
            }
            else if (SubtitlesSettings.ShowTranslation && !subtitle.IsTranslated)
            {
                subtitle.SwitchToTranslation();
            }
        }
    }

    #endregion

    #region event handlers

    async partial void OnSubtitlesSettingsChanged(SubtitlesSettings? oldValue, SubtitlesSettings newValue)
    {
        // Priority 1: Language is changed
        // OR
        // Priority 2: Translation scope increased
        if (newValue.TranslateToLanguage != oldValue?.TranslateToLanguage ||
            newValue.WhichSubtitlesToTranslate < oldValue?.WhichSubtitlesToTranslate)
        {
            await TranslateAsync();
        }

        // Priority 3: Background translation switch is toggled
        else if (newValue.ShowTranslation != oldValue?.ShowTranslation)
        {
            ApplyTranslations();
        }
    }

    #endregion
}