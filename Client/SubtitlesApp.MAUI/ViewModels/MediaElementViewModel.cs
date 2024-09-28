using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SubtitlesApp.Interfaces;

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
    TimeSpan _currentPosition;

    [ObservableProperty]
    TranscribeStatus _transcribeStatus = TranscribeStatus.NotTranscribing;
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISubtitlesService _subtitlesService;
    readonly TimeSet _coveredTimeIntervals;

    TimeInterval? _currentlyTranscribedTimeline;

    public MediaElementViewModel(
        IMediaProcessor mediaProcessor,
        ISettingsService settings,
        ISubtitlesService subtitlesService)
    {
        #region observable props

        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        TranscribeBufferLength = settings.TranscribeBufferLength;

        #endregion

        #region private props

        _mediaProcessor = mediaProcessor;
        _subtitlesService = subtitlesService;
        _coveredTimeIntervals = new TimeSet();
        _currentPosition = TimeSpan.Zero;

        #endregion

        PropertyChanged += VM_PropertyChanged;
        TranscribeCommand.CanExecuteChanged += TC_OnCanExecuteChanged;
    }

    #region public properties

    public TimeSpan MediaDuration { get; set; }

    #endregion

    #region commands

    [RelayCommand]
    public void ChangePosition(TimeSpan currentPosition)
    {
        CurrentPosition = currentPosition;

        if (ShouldTranscribe(currentPosition))
        {
            TranscribeStatus = TranscribeStatus.Transcribing;
        }
    }

    [RelayCommand]
    public void SeekTo(TimeSpan position)
    {
        (var currentInterval, _) = _coveredTimeIntervals.GetByTimeStamp(position);

        if ((currentInterval == null || !currentInterval.ContainsTime(position)) &&
            (_currentlyTranscribedTimeline == null || !_currentlyTranscribedTimeline.ContainsTime(position)))
        {
            TranscribeCommand.Cancel();
            TranscribeStatus = TranscribeStatus.NotTranscribing;
            _currentlyTranscribedTimeline = null;
        }
    }

    [RelayCommand]
    public async Task TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        _currentlyTranscribedTimeline = GetTimeIntervalForTranscription(position);

        if (_currentlyTranscribedTimeline == null)
        {
            TranscribeStatus = TranscribeStatus.NotTranscribing;
            return;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var audio = await _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                _currentlyTranscribedTimeline.StartTime,
                _currentlyTranscribedTimeline.EndTime,
                cancellationToken);

            TextBoxContent = "Transcribing...";

            var subsResult = await _subtitlesService.GetSubsAsync(audio, cancellationToken);

            if (subsResult.IsFailure)
            {
                TextBoxContent = subsResult.Error.Description;
                TranscribeStatus = TranscribeStatus.Error;
                return;
            }

            var subs = subsResult.Value;
            
            var lastAddedSub = await AddToSubsList(subs, batchLength: 30, delayMs: 20, cancellationToken);

            if (lastAddedSub == null || _currentlyTranscribedTimeline.EndTime == MediaDuration)
            {
                _coveredTimeIntervals.Insert(new TimeInterval(_currentlyTranscribedTimeline));
            }
            else
            {
                _coveredTimeIntervals.Insert(
                    new TimeInterval(
                        _currentlyTranscribedTimeline.StartTime,
                        lastAddedSub.TimeInterval.StartTime));
            }

            TranscribeStatus = TranscribeStatus.NotTranscribing;

            TextBoxContent = "Transcribing done.";
        }
        catch (OperationCanceledException)
        {
            TranscribeStatus = TranscribeStatus.NotTranscribing;
        }
        finally
        {
            _currentlyTranscribedTimeline = null;
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

    public async Task CleanAsync()
    {
        if (TranscribeCommand.CanBeCanceled)
        {
            TranscribeCommand.Cancel();
        }
        _mediaProcessor.Dispose();
    }
    #endregion

    #region private methods

    /// <summary>
    /// Adds subtitles to the list with a delay between each batch.
    /// </summary>
    /// <param name="subsToAdd"></param>
    /// <param name="batchLength"></param>
    /// <param name="delayMs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Last added subtitle</returns>
    async Task<SubtitleDTO?> AddToSubsList(
        List<SubtitleDTO> subsToAdd,
        int batchLength,
        int delayMs,
        CancellationToken cancellationToken)
    {
        int i = 0;
        SubtitleDTO? lastSub = null;

        foreach (var sub in subsToAdd)
        {
            if (i % batchLength == 0)
            {
                await Task.Delay(delayMs, cancellationToken);
            }

            AddSubtitle(sub);
            i++;

            lastSub = sub;
        }

        return lastSub;
    }

    void AddSubtitle(SubtitleDTO subtitleDTO)
    {
        var timeInterval = new TimeInterval(subtitleDTO.TimeInterval.StartTime, subtitleDTO.TimeInterval.EndTime);

        var subtitle = new Subtitle()
        {
            Text = subtitleDTO.Text,
            TimeInterval = timeInterval
        };

        Subtitles.Insert(subtitle);
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
            // start from the beginning
            startTime = TimeSpan.Zero;
        }

        var endTime = startTime.Add(TimeSpan.FromSeconds(TranscribeBufferLength));

        endTime = endTime > MediaDuration ? MediaDuration : endTime;

        return new TimeInterval(startTime, endTime);
    }

    bool ShouldTranscribe(TimeSpan position)
    {
        (var currentInterval, _) = _coveredTimeIntervals.GetByTimeStamp(position);

        // if the current interval is the last one and it covers the end of the media
        // return false
        if (currentInterval != null && currentInterval.EndTime >= MediaDuration)
        {
            return false;
        }

        var isTimeSuitableForTranscribe =
            currentInterval == null ||
            currentInterval.EndTime - position <= TimeSpan.FromSeconds(15);

        var shouldTranscribe = isTimeSuitableForTranscribe &&
            TranscribeStatus == TranscribeStatus.NotTranscribing;

        return shouldTranscribe;
    }

    #endregion

    #region event handlers
    void VM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TranscribeStatus) && TranscribeStatus == TranscribeStatus.Transcribing)
        {
            if (TranscribeCommand.CanExecute(CurrentPosition))
            {
                TranscribeCommand.ExecuteAsync(CurrentPosition);
            }
        }
    }

    void TC_OnCanExecuteChanged(object? sender, EventArgs e)
    {
        if (TranscribeStatus == TranscribeStatus.Transcribing && TranscribeCommand.CanExecute(CurrentPosition))
        {
            TranscribeCommand.ExecuteAsync(CurrentPosition);
        }
    }
    #endregion
}