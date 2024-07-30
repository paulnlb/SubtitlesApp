using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    TimeSpan _lastSeekedPosition;

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

    }

    #region public properties

    public TimeSpan MediaDuration { get; set; }

    #endregion

    #region commands

    [RelayCommand]
    public void ChangeAudioLength(string text)
    {
        if (int.TryParse(text, out int audioLength))
        {
            TranscribeBufferLength = audioLength;
            SetStatus($"Transcribe buffer set to {TranscribeBufferLength} seconds.");
        }
        else
        {
            SetStatus($"Value {text} is invalid.");
        }
    }

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
        TranscribeStatus = TranscribeStatus.NotTranscribing;

        LastSeekedPosition = position;

        Play();
    }

    [RelayCommand]
    public void SeekToSub(Subtitle subtitle)
    {
        if (subtitle != CurrentSubtitle && subtitle != null)
        {
            SeekTo(subtitle.TimeInterval.StartTime);
        }
    }

    [RelayCommand]
    public void Pause()
    {
        PlayerState = MediaPlayerStates.Paused;
    }

    [RelayCommand]
    public void Play()
    {
        PlayerState = MediaPlayerStates.Playing;
    }

    [RelayCommand]
    public void Stop()
    {
        PlayerState = MediaPlayerStates.Stopped;
    }

    [RelayCommand]
    public async Task TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        (var currentInterval, _) = _coveredTimeIntervals.GetByTimeStamp(position);

        var startTime = currentInterval == null ? position : currentInterval.EndTime;

        if (startTime >= MediaDuration)
        {
            TranscribeStatus = TranscribeStatus.NotTranscribing;
            return;
        }

        _signalrClient.CancelTranscription();

        var endTime = startTime.Add(TimeSpan.FromSeconds(TranscribeBufferLength));

        if (endTime > MediaDuration)
        {
            endTime = MediaDuration;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            (var metadata, var audioChunks) = _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                startTime,
                endTime,
                cancellationToken);

            TextBoxContent = "Sending to server...";

            await _signalrClient.SendAsync(audioChunks, metadata, cancellationToken);
        }
        catch (Exception ex)
        {
            TextBoxContent = ex.Message;
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
        _signalrClient.CancelTranscription();
        await _signalrClient.StopConnectionAsync();
        _mediaProcessor.Dispose();
    }
    #endregion

    #region private methods
    void RegisterSignalRHandlers()
    {
        Action<SubtitleDTO> showSub = ShowSubtitle;
        Action<string, TrimmedAudioMetadataDTO> setStatusWithMetadata = SetStatusAndEditTimeline;
        Action<string> setStatus = SetStatus;

        _signalrClient.RegisterHandler(nameof(ShowSubtitle), showSub);
        _signalrClient.RegisterHandler(nameof(SetStatus), setStatus);
        _signalrClient.RegisterHandler(nameof(SetStatusAndEditTimeline), setStatusWithMetadata);
    }

    void ShowSubtitle(SubtitleDTO subtitleDTO)
    {
        var timeInterval = new TimeInterval(subtitleDTO.TimeInterval.StartTime, subtitleDTO.TimeInterval.EndTime);

        var subtitle = new Subtitle()
        {
            Text = subtitleDTO.Text,
            TimeInterval = timeInterval
        };
        
        Subtitles.Insert(subtitle);

        _coveredTimeIntervals.Insert(timeInterval);
    }

    void SetStatusAndEditTimeline(string status, TrimmedAudioMetadataDTO audioMetadata)
    {
        if (status == "Done.")
        {
            var endTime = audioMetadata.EndTime;
            var startTime = audioMetadata.StartTimeOffset;

            var newInterval = new TimeInterval(startTime, endTime);

            _coveredTimeIntervals.Insert(newInterval);

            TranscribeStatus = TranscribeStatus.NotTranscribing;
        }

        TextBoxContent = status;
    }
    void SetStatus(string status)
    {
        TextBoxContent = status;
    }

    void SetCurrentSub(TimeSpan position)
    {
        if (CurrentSubtitle != null && CurrentSubtitle.TimeInterval.ContainsTime(position))
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

        var isTimeSuitableForTranscribe =
            currentInterval == null ||
            currentInterval.EndTime - position <= TimeSpan.FromSeconds(15);

        var shouldTranscribe = isTimeSuitableForTranscribe &&
            TranscribeStatus == TranscribeStatus.NotTranscribing;

        return shouldTranscribe;
    }

    private async void VM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TranscribeStatus) && TranscribeStatus == TranscribeStatus.Transcribing)
        {
            await TranscribeAsync(CurrentPosition, CancellationToken.None);
        }
    }
    #endregion
}