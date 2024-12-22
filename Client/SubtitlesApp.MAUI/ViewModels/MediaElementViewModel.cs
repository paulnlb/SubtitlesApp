using CommunityToolkit.Mvvm.ComponentModel;
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
    readonly TimeSet _coveredTimeIntervals;

    public MediaElementViewModel(
        IMediaProcessor mediaProcessor,
        ISettingsService settings,
        ISubtitlesService subtitlesService,
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService)
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
            BackgroundTranslation = true,
            WhichSubtitlesToTranslate = SubtitlesCaptureMode.VisibleAndNext,
        };

        #endregion

        #region private props

        _mediaProcessor = mediaProcessor;
        _subtitlesService = subtitlesService;
        _translationService = translationService;
        _popupService = popupService;
        _coveredTimeIntervals = new TimeSet();

        #endregion
    }

    public TimeSpan MediaDuration { get; set; }

    #region commands

    [RelayCommand]
    public void ChangePosition(TimeSpan currentPosition)
    {
        if (TranscribeStatus == TranscribeStatus.Ready
            && ShouldTranscribeFrom(currentPosition))
        {
            TranscribeFromPositionCommand.ExecuteAsync(currentPosition);
        }

        UpdateCurrentSubtitleIndex(currentPosition);
    }

    [RelayCommand]
    public void SeekTo(TimeSpan position)
    {
        if (TranscribeStatus == TranscribeStatus.Transcribing
            && ShouldTranscribeFrom(position))
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

        if (result is not SubtitlesSettings newSettings)
        {
            return;
        }

        // Priority 1: Language is changed
        // Priority 2: Translation scope increased
        if (newSettings.TranslateToLanguage != SubtitlesSettings.TranslateToLanguage ||
            newSettings.WhichSubtitlesToTranslate < SubtitlesSettings.WhichSubtitlesToTranslate)
        {
            SubtitlesSettings = newSettings;
            await TranslateAsync();
        }

        // Priority 3: Background translation switch is toggled
        else if (newSettings.BackgroundTranslation != SubtitlesSettings.BackgroundTranslation)
        {
            SubtitlesSettings = newSettings;
            ApplyTranslations();
        }

        else
        {
            SubtitlesSettings = newSettings;
        }
    }

    #endregion

    #region public methods

    VisualSubtitle? GetCurrentSubttle()
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
        var timeIntervalToTranscribe = GetTimeIntervalForTranscription(position);

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
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (subsResult.IsFailure)
            {
                return subsResult;
            }

            var subs = subsResult.Value;

            AlignSubsByTime(subs, timeIntervalToTranscribe.StartTime);

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

        var subtitlesDtos = subtitlesToTranslate.Select(vs => new SubtitleDTO
        {
            TimeInterval = new TimeIntervalDTO
            {
                StartTime = vs.TimeInterval.StartTime,
                EndTime = vs.TimeInterval.EndTime,
            },
            Text = vs.Text,
            Translation = vs.Translation,
            IsTranslated = false,
            LanguageCode = vs.LanguageCode,
        })
            .ToList();

        var request = new TranslationRequestDto
        {
            TargetLanguageCode = SubtitlesSettings.TranslateToLanguage.Code,
            SourceSubtitles = subtitlesDtos,
        };

        var subsTranslationResult = await _translationService.TranslateAsync(request);

        if (subsTranslationResult.IsSuccess)
        {
            AddToObservablesFromIndex(subsTranslationResult.Value, startingIndex, !SubtitlesSettings.BackgroundTranslation);
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
            var timeInterval = new TimeInterval(
                subtitleDto.TimeInterval.StartTime, 
                subtitleDto.TimeInterval.EndTime);

            var subtitle = new VisualSubtitle()
            {
                Text = subtitleDto.Text,
                TimeInterval = timeInterval,
                LanguageCode = subtitleDto.LanguageCode,
                Translation = subtitleDto.Translation,
            };

            Subtitles.Insert(subtitle);
        }
    }

    void AddToObservablesFromIndex(
        IEnumerable<SubtitleDTO> subsToAdd,
        int insertIndex,
        bool applyTranslation)
    {
        foreach (var subtitleDto in subsToAdd)
        {
            var timeInterval = new TimeInterval(
                subtitleDto.TimeInterval.StartTime,
                subtitleDto.TimeInterval.EndTime);

            var subtitle = new VisualSubtitle()
            {
                Text = subtitleDto.Text,
                TimeInterval = timeInterval,
                LanguageCode = subtitleDto.LanguageCode,
                Translation = subtitleDto.Translation,
            };

            if (applyTranslation)
            {
                subtitle.ApplyTranslation();
            }

            Subtitles[insertIndex] = subtitle;
            insertIndex++;
        }
    }

    static void AlignSubsByTime(
        List<SubtitleDTO> subsToAlign,
        TimeSpan timeOffset)
    {
        foreach (var subtitleDto in subsToAlign)
        {
            var timeInterval = new TimeIntervalDTO
            {
                StartTime = subtitleDto.TimeInterval.StartTime + timeOffset,
                EndTime = subtitleDto.TimeInterval.EndTime + timeOffset,
            };

            subtitleDto.TimeInterval = timeInterval;
        }
    }

    TimeInterval? GetTimeIntervalForTranscription(TimeSpan position)
    {
        (var currentInterval, _) = _coveredTimeIntervals.GetByTimeStamp(position);

        var startTime = currentInterval == null ? position : currentInterval.EndTime;

        if (startTime >= MediaDuration)
        {
            return null;
        }

        if (startTime <= TimeSpan.FromSeconds(1))
        {
            // Start from the beginning
            startTime = TimeSpan.Zero;
        }

        var endTime = startTime.Add(TimeSpan.FromSeconds(TranscribeBufferLength));

        if (endTime > MediaDuration)
        {
            endTime = MediaDuration;
        }

        return new TimeInterval(startTime, endTime);
    }

    bool ShouldTranscribeFrom(TimeSpan position)
    {
        (var currentInterval, _) = _coveredTimeIntervals.GetByTimeStamp(position);

        // If the current interval is the last one and it covers the end of the media
        // return false
        if (currentInterval != null && currentInterval.EndTime >= MediaDuration)
        {
            return false;
        }

        var isTimeSuitableForTranscribe =
            currentInterval == null ||
            currentInterval.EndTime - position <= TimeSpan.FromSeconds(15);

        return isTimeSuitableForTranscribe;
    }

    void UpdateCurrentSubtitleIndex(TimeSpan currentPosition)
    {
        var currentSubtitle = GetCurrentSubttle();

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
            if (SubtitlesSettings.BackgroundTranslation && subtitle.IsTranslated)
            {
                subtitle.ApplyTranslation();
            }
            else if (!SubtitlesSettings.BackgroundTranslation && !subtitle.IsTranslated)
            {
                subtitle.ApplyTranslation();
            }
        }
    }

    #endregion
}