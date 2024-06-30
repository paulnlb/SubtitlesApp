﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Core.Models;
using System.Collections.ObjectModel;

namespace SubtitlesApp.ViewModels;

public partial class MediaElementViewModel : ObservableObject, IQueryAttributable
{
    #region observable properties
    [ObservableProperty]
    ICollection<string> _shownSubtitlesText;

    [ObservableProperty]
    double _scrollViewHeight;

    [ObservableProperty]
    string _textBoxContent;

    [ObservableProperty]
    string? _mediaPath;

    [ObservableProperty]
    int _transcribeBufferLength;
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISignalRClient _signalrClient;

    public MediaElementViewModel(
        ISignalRClient signalRClient,
        IMediaProcessor mediaProcessor)
    {
        #region observable props

        ScrollViewHeight = 200;
        TextBoxContent = "";
        MediaPath = null;
        ShownSubtitlesText = new ObservableCollection<string>();
        Subtitles = new List<Subtitle>();
        ShownSubtitles = new List<Subtitle>();
        TranscribeBufferLength = 30;

        #endregion

        #region private props

        _signalrClient = signalRClient;
        _mediaProcessor = mediaProcessor;

        #endregion
    }

    #region public properties
    public ICollection<Subtitle> Subtitles { get; set; }

    public ICollection<Subtitle> ShownSubtitles { get; set; }
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
    public void ClearSubtitles()
    {
        ShownSubtitles.Clear();
        ShownSubtitlesText.Clear();
    }

    [RelayCommand]
    public async Task TranscribeAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        _signalrClient.CancelTranscription();

        ClearSubtitles();

        TextBoxContent = "Sending to server...";

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            (var metadata, var audioChunks) = _mediaProcessor.ExtractAudioAsync(
                MediaPath,
                position,
                TranscribeBufferLength,
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
        Action<Subtitle> showSub = ShowSubtitle;
        Action<string> setStatus = SetStatus;

        _signalrClient.RegisterHandler(nameof(ShowSubtitle), showSub);
        _signalrClient.RegisterHandler(nameof(SetStatus), setStatus);
    }

    void ShowSubtitle(Subtitle subtitle)
    {
        Subtitles.Add(subtitle);
    }

    void SetStatus(string status)
    {
        TextBoxContent = status;
    }
    #endregion
}