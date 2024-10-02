﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Extensions;
using System.Collections.ObjectModel;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Core.Result;
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
    #endregion

    readonly IMediaProcessor _mediaProcessor;
    readonly ISubtitlesService _subtitlesService;
    readonly TimeSet _coveredTimeIntervals;

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

        #endregion
    }

    #region public properties

    public TimeSpan MediaDuration { get; set; }

    #endregion

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

            var subsResult = await _subtitlesService.GetSubsAsync(audio, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (subsResult.IsFailure)
            {
                return Result.Failure(subsResult.Error);
            }

            var subs = subsResult.Value;
            
            AddToObservableList(subs);

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
    /// Adds subtitles to the list.
    /// </summary>
    /// <param name="subsToAdd"></param>
    /// <returns>Last added subtitle</returns>
    void AddToObservableList(
        List<SubtitleDTO> subsToAdd)
    {
        foreach (var subtitleDto in subsToAdd)
        {
            var timeInterval = new TimeInterval(subtitleDto.TimeInterval.StartTime, subtitleDto.TimeInterval.EndTime);

            var subtitle = new Subtitle()
            {
                Text = subtitleDto.Text,
                TimeInterval = timeInterval
            };

            Subtitles.Insert(subtitle);
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
            // start from the beginning
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

        // if the current interval is the last one and it covers the end of the media
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