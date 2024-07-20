using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Application.Interfaces;
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
    double _scrollViewHeight;

    [ObservableProperty]
    string _textBoxContent;

    [ObservableProperty]
    string? _mediaPath;

    [ObservableProperty]
    int _transcribeBufferLength;

    [ObservableProperty]
    Subtitle _currentSubtitle;
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISignalRClient _signalrClient;
    readonly List<TimeInterval> _coveredTimeIntervals;

    public MediaElementViewModel(
        ISignalRClient signalRClient,
        IMediaProcessor mediaProcessor,
        ISettingsService settings)
    {
        #region observable props

        ScrollViewHeight = 200;
        TextBoxContent = "";
        MediaPath = null;
        Subtitles = [];
        TranscribeBufferLength = settings.TranscribeBufferLength;

        #endregion

        #region private props

        _signalrClient = signalRClient;
        _mediaProcessor = mediaProcessor;
        _coveredTimeIntervals = [];

        #endregion
    }

    #region public properties
    public List<TimeInterval> CoveredTimeIntervals => _coveredTimeIntervals;

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
    public async Task TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        _signalrClient.CancelTranscription();

        var endTime = position.Add(TimeSpan.FromSeconds(TranscribeBufferLength));

        if (endTime > MediaDuration)
        {
            endTime = MediaDuration;
        }

        TextBoxContent = "Sending to server...";

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            (var metadata, var audioChunks) = _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                position,
                endTime,
                cancellationToken);

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
    }

    void SetStatusAndEditTimeline(string status, TrimmedAudioMetadataDTO audioMetadata)
    {
        if (status == "Done.")
        {
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

    public (bool ShouldTranscribe, TimeSpan? TranscribeStartTime) ShouldTranscribe(TimeSpan position)
    {
        (var currentTimeInterval, _) = CoveredTimeIntervals.BinarySearch(position);

        var isTimeSuitableForTranscribe = 
            currentTimeInterval == null || 
            currentTimeInterval.EndTime - position <= TimeSpan.FromSeconds(15);

        TimeSpan? transcribeStartTime = currentTimeInterval == null ? position : currentTimeInterval.EndTime;

        if (transcribeStartTime >= MediaDuration)
        {
            isTimeSuitableForTranscribe = false;
        }

        var shouldTranscribe = isTimeSuitableForTranscribe &&
            TextBoxContent != "Transcribing..." &&
            TextBoxContent != "Sending to server...";

        if (!shouldTranscribe)
        {
            transcribeStartTime = null;
        }

        return (shouldTranscribe, transcribeStartTime);
    }
    #endregion
}