using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using SubtitlesApp.Shared.Extensions;
using System.Collections.ObjectModel;

namespace SubtitlesApp.ViewModels;

public partial class MediaElementViewModel : ObservableObject, IQueryAttributable
{
    #region observable properties

    [ObservableProperty]
    ObservableCollection<Subtitle> _subtitles;

    [ObservableProperty]
    ObservableCollection<Subtitle> _shownSubtitles;

    [ObservableProperty]
    string _textBoxContent;

    [ObservableProperty]
    string? _mediaPath;

    [ObservableProperty]
    int _transcribeBufferLength;

    [ObservableProperty]
    Subtitle? _currentSubtitle;

    [ObservableProperty]
    TimeSpan _lastSeekedPosition;

    [ObservableProperty]
    MediaPlayerStates _playerState;
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISignalRClient _signalrClient;
    readonly TimeSet _coveredTimeIntervals;

    TimeSpan _currentPosition;
    TranscribeStatus _transcribeStatus = TranscribeStatus.NotTranscribing;

    public MediaElementViewModel(
        ISignalRClient signalRClient,
        IMediaProcessor mediaProcessor,
        ISettingsService settings)
    {
        #region observable props

        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        ShownSubtitles = [];
        TranscribeBufferLength = settings.TranscribeBufferLength;

        #endregion

        #region private props

        _signalrClient = signalRClient;
        _mediaProcessor = mediaProcessor;
        _coveredTimeIntervals = new TimeSet();
        _currentPosition = TimeSpan.Zero;

        #endregion
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
    public async Task ChangePositionAsync(TimeSpan currentPosition)
    {
        _currentPosition = currentPosition;

        (var shouldTranscribe, var transcribeStartTime) = ShouldTranscribe(currentPosition);

        if (shouldTranscribe && transcribeStartTime != null)
        {
            _transcribeStatus = TranscribeStatus.Transcribing;

            await TranscribeAsync(transcribeStartTime.Value, CancellationToken.None);
        }
        else
        {
            SetCurrentSub(currentPosition);
        }
    }

    [RelayCommand]
    public void SeekTo(TimeSpan position)
    {
        _transcribeStatus = TranscribeStatus.NotTranscribing;

        LastSeekedPosition = position;

        Play();
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

        if (subtitle.TimeInterval.StartTime <= _currentPosition)
        {
            ShownSubtitles.Insert(subtitle);
        }
        else
        {
            Subtitles.Insert(subtitle);
        }

        _coveredTimeIntervals.Insert(timeInterval);
    }

    void SetStatusAndEditTimeline(string status, TrimmedAudioMetadataDTO audioMetadata)
    {
        if (status == "Done.")
        {
            _transcribeStatus = TranscribeStatus.NotTranscribing;

            var endTime = audioMetadata.EndTime;
            var startTime = audioMetadata.StartTimeOffset;

            var newInterval = new TimeInterval(startTime, endTime);

            _coveredTimeIntervals.Insert(newInterval);
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

        // check if the subtitle is already shown
        (var shownSubtitle, _) = ShownSubtitles.BinarySearch(position);

        // if it is, highlight it
        if (shownSubtitle != null)
        {
            if (CurrentSubtitle != null)
            {
                CurrentSubtitle.IsHighlighted = false;
            }

            shownSubtitle.IsHighlighted = true;

            CurrentSubtitle = shownSubtitle;
        }

        // if not, search in the non-shown subtitles collection
        if (shownSubtitle == null)
        {
            (var nonShownSubtitle, var nonShownIndex) = Subtitles.BinarySearch(position);

            // if found, highlight it and move it to the shown collection
            if (nonShownSubtitle != null)
            {
                if (CurrentSubtitle != null)
                {
                    CurrentSubtitle.IsHighlighted = false;
                }

                nonShownSubtitle.IsHighlighted = true;

                CurrentSubtitle = nonShownSubtitle;

                ShownSubtitles.Insert(nonShownSubtitle);
                Subtitles.RemoveAt(nonShownIndex);
            }
        }
    }

    (bool ShouldTranscribe, TimeSpan? TranscribeStartTime) ShouldTranscribe(TimeSpan position)
    {
        (var currentTimeInterval, _) = _coveredTimeIntervals.GetByTimeStamp(position);

        var isTimeSuitableForTranscribe =
            currentTimeInterval == null ||
            currentTimeInterval.EndTime - position <= TimeSpan.FromSeconds(15);

        TimeSpan? transcribeStartTime = currentTimeInterval == null ? position : currentTimeInterval.EndTime;

        if (transcribeStartTime >= MediaDuration)
        {
            isTimeSuitableForTranscribe = false;
        }

        var shouldTranscribe = isTimeSuitableForTranscribe &&
            _transcribeStatus == TranscribeStatus.NotTranscribing;

        if (!shouldTranscribe)
        {
            transcribeStartTime = null;
        }

        return (shouldTranscribe, transcribeStartTime);
    }

    async Task TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        _signalrClient.CancelTranscription();

        var endTime = position.Add(TimeSpan.FromSeconds(TranscribeBufferLength));

        if (endTime > MediaDuration)
        {
            endTime = MediaDuration;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            (var metadata, var audioChunks) = _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                position,
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
}