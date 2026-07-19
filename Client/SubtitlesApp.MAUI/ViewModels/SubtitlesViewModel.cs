using System.Collections.ObjectModel;
using Android.Telephony.Mbms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Extensions;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Mapper;
using SubtitlesApp.Services;

namespace SubtitlesApp.ViewModels;

public partial class SubtitlesViewModel : ObservableObject
{
    #region observable properties

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _subtitles;

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _translations;

    [ObservableProperty]
    private int _currentSubtitleIndex = -1;

    [ObservableProperty]
    private int _currentTranslationIndex = -1;

    [ObservableProperty]
    private TimeSpan _mediaDuration;

    [ObservableProperty]
    private bool _isTranscriptionLoading;

    [ObservableProperty]
    private bool _isTranslationLoading;

    [ObservableProperty]
    private MediaFileInfo? _fileInfo;

    #endregion

    #region services

    private readonly ITranslationService _translationService;
    private readonly ICustomPopupService _popupService;
    private readonly ITranscriptionService _transcriptionService;
    private readonly IBuiltInDialogService _builtInDialogService;
    private readonly LanguageService _languageService;
    private readonly LocalFileManager _localFileManager;

    #endregion

    #region private fields

    private TranscriptionSettings? _transcriptionSettings;
    private TranslationSettings? _translationSettings;
    private TimeSpan _currentMediaTime;

    #endregion

    #region events

    public event Func<Task>? SubtitlesUpdated;
    public event Func<Task>? TranslationsUpdated;

    #endregion

    public SubtitlesViewModel(
        ITranslationService translationService,
        LanguageService languageService,
        ICustomPopupService popupService,
        ITranscriptionService transcriptionService,
        IBuiltInDialogService builtInDialogService,
        LocalFileManager localFileManager
    )
    {
        #region observable properties

        Subtitles = [];
        Translations = [];

        #endregion

        _translationService = translationService;
        _popupService = popupService;
        _transcriptionService = transcriptionService;
        _builtInDialogService = builtInDialogService;
        _languageService = languageService;
        _localFileManager = localFileManager;
    }

    #region commands

    [RelayCommand]
    public void UpdateIndexes(TimeSpan currentPosition)
    {
        _currentMediaTime = currentPosition;
        CurrentSubtitleIndex = FindNewIndex(currentPosition, Subtitles, CurrentSubtitleIndex);
        CurrentTranslationIndex = FindNewIndex(currentPosition, Translations, CurrentTranslationIndex);
    }

    [RelayCommand]
    public async Task Transcribe(CancellationToken cancellationToken)
    {
        var subtitlesLang = _transcriptionSettings?.SubtitlesLanguage ?? _languageService.GetDefaultLanguage();

        var popupResult = await _popupService.ShowTranscriptionSettings(
            MediaDuration,
            _currentMediaTime,
            subtitlesLang,
            _transcriptionSettings?.FromTime,
            _transcriptionSettings?.ToTime
        );

        if (popupResult is not TranscriptionSettings newSettings)
        {
            return;
        }

        _transcriptionSettings = newSettings;
        IsTranscriptionLoading = true;

        var timeInterval = new TimeInterval(newSettings.FromTime, newSettings.ToTime);

        Subtitles.RemoveInside(timeInterval);

        IAsyncEnumerable<Result<Subtitle>> results;
        Stream? stream = null;

        if (FileInfo.Type == FileResourceType.Remote)
        {
            results = _transcriptionService.TranscribeAsync(
                FileInfo.Uri,
                timeInterval,
                newSettings.SubtitlesLanguage.Code,
                cancellationToken
            );
        }
        else
        {
            var streamResult = _localFileManager.GetFileStream(FileInfo.Uri);

            if (streamResult.IsFailure)
            {
                await _builtInDialogService.DisplayError(streamResult.Error);
            }

            stream = streamResult.Value;

            results = _transcriptionService.TranscribeAsync(
                stream,
                timeInterval,
                newSettings.SubtitlesLanguage.Code,
                cancellationToken
            );
        }

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                await _builtInDialogService.DisplayError(result.Error);

                IsTranscriptionLoading = false;

                return;
            }

            var subtitle = result.Value;

            // Workaround that reduces timestamp precision to roughly match seeking precision
            var newStart = TimeSpan.FromMilliseconds(Math.Round(subtitle.TimeInterval.StartTime.TotalMilliseconds));
            var newEnd = TimeSpan.FromMilliseconds(Math.Round(subtitle.TimeInterval.EndTime.TotalMilliseconds));
            subtitle.TimeInterval = new TimeInterval(newStart, newEnd);

            Subtitles.Insert(SubtitlesMapper.ToVisualSubtitle(subtitle), NeighborRemovalMode.FullOverlap);
        }

        stream?.Dispose();

        try
        {
            await InvokeAsync(SubtitlesUpdated);
        }
        catch (Exception ex)
        {
            var error = new Error(ErrorCode.SubtitlesPersistenceError, ex.Message);
            await _builtInDialogService.DisplayError(error);
        }
        finally
        {
            IsTranscriptionLoading = false;
        }
    }

    [RelayCommand]
    public async Task Translate(CancellationToken cancellationToken)
    {
        var popupResult = await _popupService.ShowTranslationSettings(
            MediaDuration,
            _translationSettings?.TargetLanguage,
            _translationSettings?.FromTime,
            _translationSettings?.ToTime
        );

        if (popupResult is not TranslationSettings newSettings)
        {
            return;
        }

        _translationSettings = newSettings;

        var subtitlesToTranslate = Subtitles.Where(s =>
            s.TimeInterval.StartTime >= newSettings.FromTime && s.TimeInterval.EndTime <= newSettings.ToTime
        );

        var subtitles = SubtitlesMapper.ToSubtitleList(subtitlesToTranslate);

        IsTranslationLoading = true;

        Translations.RemoveInside(new(newSettings.FromTime, newSettings.ToTime));

        var results = _translationService.TranslateAsync(subtitles, newSettings.TargetLanguage, cancellationToken);

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                await _builtInDialogService.DisplayError(result.Error);

                IsTranslationLoading = false;

                return;
            }

            Translations.Insert(SubtitlesMapper.ToVisualSubtitle(result.Value), NeighborRemovalMode.FullOverlap);
        }

        try
        {
            await InvokeAsync(TranslationsUpdated);
        }
        catch (Exception ex)
        {
            var error = new Error(ErrorCode.SubtitlesPersistenceError, ex.Message);
            await _builtInDialogService.DisplayError(error);
        }
        finally
        {
            IsTranslationLoading = false;
        }
    }

    #endregion

    private static int FindNewIndex(TimeSpan currPosition, ObservableCollection<VisualSubtitle> subtitles, int currIndex)
    {
        if (subtitles is null or { Count: 0 })
        {
            return -1;
        }

        if (currIndex == -1)
        {
            var (_, i) = subtitles.BinarySearch(currPosition);

            return i;
        }

        var currSub = subtitles[currIndex];

        if (currSub.TimeInterval.ContainsTime(currPosition))
        {
            return currIndex;
        }

        VisualSubtitle? prevSub = currIndex > 0 ? subtitles[currIndex - 1] : null;
        VisualSubtitle? nextSub = currIndex < subtitles.Count - 1 ? subtitles[currIndex + 1] : null;

        int newIndex;

        if (prevSub is not null && prevSub.TimeInterval.ContainsTime(currPosition))
        {
            newIndex = currIndex - 1;
        }
        else if (nextSub is not null && nextSub.TimeInterval.ContainsTime(currPosition))
        {
            newIndex = currIndex + 1;
        }
        else
        {
            (_, newIndex) = subtitles.BinarySearch(currPosition);
        }

        return newIndex;
    }

    private static async Task InvokeAsync(Func<Task>? eventToInvoke)
    {
        if (eventToInvoke is null)
        {
            return;
        }

        var handlers = eventToInvoke.GetInvocationList();

        foreach (var handler in handlers.Cast<Func<Task>>())
        {
            await handler();
        }
    }
}
