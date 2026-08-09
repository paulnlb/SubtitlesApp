using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Constants;
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
    #region public properties
    public string? CachedSubtitlesFile { get; set; }
    public string? CachedTranslationsFile { get; set; }
    #endregion

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
    private readonly ISubtitlesCache _subtitlesCache;
    private readonly ILogger<SubtitlesViewModel> _logger;

    #endregion

    #region private fields

    private TranscriptionSettings? _transcriptionSettings;
    private TranslationSettings? _translationSettings;
    private TimeSpan _currentMediaTime;

    #endregion

    public SubtitlesViewModel(
        ITranslationService translationService,
        LanguageService languageService,
        ICustomPopupService popupService,
        ITranscriptionService transcriptionService,
        IBuiltInDialogService builtInDialogService,
        LocalFileManager localFileManager,
        ISubtitlesCache subtitlesCache,
        ILogger<SubtitlesViewModel> logger
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
        _subtitlesCache = subtitlesCache;
        _logger = logger;
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

        var totalResult = Result.Success();
        var anyGenerated = false;

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                totalResult = Result.Failure(result.Error);
                break;
            }

            var subtitle = result.Value;

            // Workaround that reduces timestamp precision to roughly match seeking precision
            var newStart = TimeSpan.FromMilliseconds(Math.Round(subtitle.TimeInterval.StartTime.TotalMilliseconds));
            var newEnd = TimeSpan.FromMilliseconds(Math.Round(subtitle.TimeInterval.EndTime.TotalMilliseconds));
            subtitle.TimeInterval = new TimeInterval(newStart, newEnd);

            Subtitles.Insert(SubtitlesMapper.ToVisualSubtitle(subtitle), NeighborRemovalMode.FullOverlap);

            anyGenerated = true;
        }

        stream?.Dispose();

        if (totalResult.IsSuccess)
        {
            await ApplySubtitlesAction(SubtitlesActionConstants.Save);
            IsTranscriptionLoading = false;

            return;
        }

        if (totalResult.Error.Code != ErrorCode.OperationCanceled)
        {
            await _builtInDialogService.DisplayError(totalResult.Error);
        }

        if (anyGenerated)
        {
            var action = await GetActionOnPartiallyGenerated();
            await ApplySubtitlesAction(action);
        }
        else
        {
            await ApplySubtitlesAction(SubtitlesActionConstants.Restore);
        }

        IsTranscriptionLoading = false;
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

        var totalResult = Result.Success();
        var anyGenerated = false;

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                totalResult = Result.Failure(result.Error);
                break;
            }

            Translations.Insert(SubtitlesMapper.ToVisualSubtitle(result.Value), NeighborRemovalMode.FullOverlap);
            anyGenerated = true;
        }

        if (totalResult.IsSuccess)
        {
            await ApplyTranslationsAction(SubtitlesActionConstants.Save);
            IsTranslationLoading = false;

            return;
        }

        if (totalResult.Error.Code != ErrorCode.OperationCanceled)
        {
            await _builtInDialogService.DisplayError(totalResult.Error);
        }

        if (anyGenerated)
        {
            var action = await GetActionOnPartiallyGenerated();
            await ApplyTranslationsAction(action);
        }
        else
        {
            await ApplyTranslationsAction(SubtitlesActionConstants.Restore);
        }

        IsTranslationLoading = false;
    }

    #endregion

    #region public methods

    public async Task LoadSubtitlesFromCache()
    {
        IsTranscriptionLoading = true;

        if (string.IsNullOrWhiteSpace(CachedSubtitlesFile))
        {
            _logger.LogWarning("Cannot load subtitles from cache: no file name is specified");
        }
        else
        {
            var items = await _subtitlesCache.Get(CachedSubtitlesFile);

            if (items?.Any() == true)
            {
                Subtitles = SubtitlesMapper.ToVisualSubtitles(items);
            }
        }

        IsTranscriptionLoading = false;
    }

    public async Task LoadTranslationsFromCache()
    {
        IsTranslationLoading = true;

        if (string.IsNullOrWhiteSpace(CachedTranslationsFile))
        {
            _logger.LogWarning("Cannot load translations from cache: no file name is specified");
        }
        else
        {
            var items = await _subtitlesCache.Get(CachedTranslationsFile);

            if (items?.Any() == true)
            {
                Translations = SubtitlesMapper.ToVisualSubtitles(items);
            }
        }

        IsTranslationLoading = false;
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

    private async Task<string> GetActionOnPartiallyGenerated()
    {
        var options = new List<PickerItem>
        {
            new() { Title = "Keep new (old items will be lost)", Action = SubtitlesActionConstants.Save },
            new() { Title = "Discard new, restore old", Action = SubtitlesActionConstants.Restore },
            new() { Title = "Keep new until the video is closed", Action = SubtitlesActionConstants.DoNothing },
        };

        var result = await _popupService.ShowRadioButtons(
            "Partially completed",
            options,
            x => x.Title,
            options[0],
            "Items generation was interrupted by an error or cancelled. Select one of the actions below to proceed.",
            false
        );

        if (result is null)
        {
            return SubtitlesActionConstants.DoNothing;
        }
        else
        {
            return result.Action;
        }
    }

    private async Task ApplySubtitlesAction(string action)
    {
        switch (action)
        {
            case SubtitlesActionConstants.DoNothing:
                return;

            case SubtitlesActionConstants.Save when !string.IsNullOrWhiteSpace(CachedSubtitlesFile):
                await _subtitlesCache.Save(CachedSubtitlesFile, Subtitles);
                break;

            case SubtitlesActionConstants.Restore when !string.IsNullOrWhiteSpace(CachedSubtitlesFile):
                var subtitles = await _subtitlesCache.Get(CachedSubtitlesFile) ?? [];
                Subtitles = SubtitlesMapper.ToVisualSubtitles(subtitles);
                break;

            default:
                _logger.LogWarning(
                    "Although the \"{ActionName}\" action was provided, no subtitles were updated because none of the conditions was met",
                    action
                );
                break;
        }
    }

    private async Task ApplyTranslationsAction(string action)
    {
        switch (action)
        {
            case SubtitlesActionConstants.DoNothing:
                return;

            case SubtitlesActionConstants.Save when !string.IsNullOrWhiteSpace(CachedTranslationsFile):
                await _subtitlesCache.Save(CachedTranslationsFile, Subtitles);
                break;

            case SubtitlesActionConstants.Restore when !string.IsNullOrWhiteSpace(CachedTranslationsFile):
                var subtitles = await _subtitlesCache.Get(CachedTranslationsFile) ?? [];
                Translations = SubtitlesMapper.ToVisualSubtitles(subtitles);
                break;

            default:
                _logger.LogWarning(
                    "Although the \"{ActionName}\" action was provided, no subtitles were updated because none of the conditions was met",
                    action
                );
                break;
        }
    }
}
