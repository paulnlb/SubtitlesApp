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
namespace SubtitlesApp.ViewModels;

public partial class MediaElementViewModel : ObservableObject, IQueryAttributable
{
    #region observable properties

    [ObservableProperty]
    ObservableCollection<Subtitle> _subtitles;

    [ObservableProperty]
    string _textBoxContent;

    [ObservableProperty]
    string? _mediaPath;

    [ObservableProperty]
    int _transcribeBufferLength;

    [ObservableProperty]
    TranscribeStatus _transcribeStatus = TranscribeStatus.Ready;

    [ObservableProperty]
    List<Language> _availableSubtitlesLanguages;

    [ObservableProperty]
    Language _selectedSubtitlesLanguage;
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISubtitlesService _subtitlesService;
    readonly TimeSet _coveredTimeIntervals;

    public MediaElementViewModel(
        IMediaProcessor mediaProcessor,
        ISettingsService settings,
        ISubtitlesService subtitlesService,
        LanguageService languageService)
    {
        #region observable props

        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        TranscribeBufferLength = settings.TranscribeBufferLength;
        AvailableSubtitlesLanguages = languageService.GetAllLanguages();
        SelectedSubtitlesLanguage = languageService.GetDefaultLanguage();

        #endregion

        #region private props

        _mediaProcessor = mediaProcessor;
        _subtitlesService = subtitlesService;
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

    public async Task<Result> TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        var timeIntervalToTranslate = GetTimeIntervalForTranscription(position);

        if (timeIntervalToTranslate == null)
        {
            return Result.Success();
        }

        try
        {
            var audio = await _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                timeIntervalToTranslate.StartTime,
                timeIntervalToTranslate.EndTime,
                cancellationToken);

            var subsResult = await _subtitlesService.GetSubsAsync(audio, SelectedSubtitlesLanguage, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (subsResult.IsFailure)
            {
                return subsResult;
            }

            var subs = subsResult.Value;

            subs = AlignSubsByTime(subs, timeIntervalToTranslate.StartTime);
            
            AddToObservables(subs);

            var lastAddedSub = subs.LastOrDefault();

            if (lastAddedSub == null || timeIntervalToTranslate.EndTime == MediaDuration)
            {
                _coveredTimeIntervals.Insert(
                    new TimeInterval(timeIntervalToTranslate));
            }
            else
            {
                _coveredTimeIntervals.Insert(
                    new TimeInterval(
                        timeIntervalToTranslate.StartTime,
                        lastAddedSub.TimeInterval.StartTime));
            }

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Success();
        }
    }
    #endregion

    #region public methods
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

            var subtitle = new Subtitle()
            {
                Text = subtitleDto.Text,
                TimeInterval = timeInterval
            };

            Subtitles.Insert(subtitle);
        }
    }

    static List<SubtitleDTO> AlignSubsByTime(
        List<SubtitleDTO> subsToAlign,
        TimeSpan timeOffset)
    {
        var result = new List<SubtitleDTO>();

        foreach (var subtitleDto in subsToAlign)
        {
            var timeInterval = new TimeIntervalDTO
            {
                StartTime = subtitleDto.TimeInterval.StartTime + timeOffset,
                EndTime = subtitleDto.TimeInterval.EndTime + timeOffset,
            };

            result.Add(new SubtitleDTO
            {
                Text = subtitleDto.Text,
                TimeInterval = timeInterval
            });
        }

        return result;
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

    #endregion
}