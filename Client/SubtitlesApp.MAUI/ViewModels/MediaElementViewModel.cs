using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiPageFullScreen;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using SubtitlesApp.Shared.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
    int _currentSubtitleIndex = -1;

    [ObservableProperty]
    Subtitle? _currentSubtitle;

    [ObservableProperty]
    MediaPlayerStates _playerState;

    [ObservableProperty]
    TimeSpan _currentPosition;

    [ObservableProperty]
    TranscribeStatus _transcribeStatus = TranscribeStatus.NotTranscribing;
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISignalRClient _signalrClient;
    readonly TimeSet _coveredTimeIntervals;

    TimeInterval? _timelineBeingTranscribed;

    public MediaElementViewModel(
        ISignalRClient signalRClient,
        IMediaProcessor mediaProcessor,
        ISettingsService settings)
    {
        #region observable props

        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        TranscribeBufferLength = settings.TranscribeBufferLength;

        #endregion

        #region private props

        _signalrClient = signalRClient;
        _mediaProcessor = mediaProcessor;
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
        else
        {
            SetCurrentSub(currentPosition);
        }
    }

    [RelayCommand]
    public void SeekTo(TimeSpan position)
    {
        (var currentInterval, _) = _coveredTimeIntervals.GetByTimeStamp(position);

        if ((currentInterval == null || !currentInterval.ContainsTime(position)) &&
            (_timelineBeingTranscribed == null || !_timelineBeingTranscribed.ContainsTime(position)))
        {
            TranscribeCommand.Cancel();
            TranscribeStatus = TranscribeStatus.NotTranscribing;
            _timelineBeingTranscribed = null;
        }
    }

    [RelayCommand]
    public void ToggleFullScreen()
    {
        Controls.ToggleFullScreenStatus();
    }

    [RelayCommand]
    public async Task TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        _timelineBeingTranscribed = GetTimeIntervalForTranscription(position);

        if (_timelineBeingTranscribed == null)
        {
            TranscribeStatus = TranscribeStatus.NotTranscribing;
            return;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            (var metadata, var audioChunks) = _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                _timelineBeingTranscribed.StartTime,
                _timelineBeingTranscribed.EndTime,
                cancellationToken);

            TextBoxContent = "Sending to server...";

            var subs = _signalrClient.StreamAsync(audioChunks, metadata, cancellationToken);

            await AddToSubsList(subs, batchLength: 30, delayMs: 20, cancellationToken);

            _coveredTimeIntervals.Insert(new TimeInterval(_timelineBeingTranscribed));
        }
        catch (Exception ex)
        {
            TextBoxContent = ex.Message;
        }
        finally
        {
            _timelineBeingTranscribed = null;

            TranscribeStatus = TranscribeStatus.NotTranscribing;
        }
    }
    #endregion

    #region public methods
    async void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("open", out object? value))
        {
            MediaPath = value.ToString();
        }

        if (MediaPath != null)
        {
            RegisterSignalRHandlers();
            await ConnectToServerAsync();
        }
    }

    // preserve as a future command for manual connection
    public async Task ConnectToServerAsync()
    {
        var (isConnected, message) = await _signalrClient.TryConnectAsync();

        if (isConnected)
        {
            TextBoxContent = message;
        }
        else
        {
            TextBoxContent = "Connection error:" + message;
        }
    }

    public async Task CleanAsync()
    {
        if (TranscribeCommand.CanBeCanceled)
        {
            TranscribeCommand.Cancel();
        }
        await _signalrClient.StopConnectionAsync();
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
    /// <returns></returns>
    async Task AddToSubsList(
        IAsyncEnumerable<SubtitleDTO> subsToAdd,
        int batchLength,
        int delayMs,
        CancellationToken cancellationToken)
    {
        int i = 0;
        await foreach (var sub in subsToAdd)
        {
            if (i % batchLength == 0)
            {
                await Task.Delay(delayMs, cancellationToken);
            }

            AddSubtitle(sub);
            i++;
        }
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

    void RegisterSignalRHandlers()
    {
        Action<string> setStatus = SetStatus;

        _signalrClient.RegisterHandler(nameof(SetStatus), setStatus);
    }

    void SetStatus(string status)
    {
        TextBoxContent = status;
    }

    void SetCurrentSub(TimeSpan position)
    {
        if (CurrentSubtitle?.TimeInterval.ContainsTime(position) == true)
        {
            return;
        }

        (var sub, var index) = Subtitles.BinarySearch(position);

        if (sub != null)
        {
            // highlight the current subtitle
            if (CurrentSubtitle != null)
            {
                CurrentSubtitle.IsHighlighted = false;
            }

            sub.IsHighlighted = true;

            CurrentSubtitle = sub;
            CurrentSubtitleIndex = index;
        }
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