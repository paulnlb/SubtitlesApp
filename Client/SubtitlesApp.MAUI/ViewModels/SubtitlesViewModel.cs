using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Extensions;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Mapper;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.ViewModels;

public partial class SubtitlesViewModel : ObservableObject
{
    #region observable properties

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _subtitles;

    [ObservableProperty]
    private ObservableCollection<VisualSubtitle> _translations;

    [ObservableProperty]
    private string? _mediaPath;

    [ObservableProperty]
    private int _currentSubtitleIndex;

    [ObservableProperty]
    private int _currentTranslationIndex;

    [ObservableProperty]
    private TimeSpan _mediaDuration;

    #endregion

    #region services

    private readonly ITranslationService _translationService;
    private readonly IPopupService _popupService;
    private readonly ITranscriptionService _transcriptionService;
    private readonly IBuiltInDialogService _builtInDialogService;
    private readonly LanguageService _languageService;

    #endregion

    #region private fields

    private readonly SubtitlesMapper _subtitlesMapper;
    private TranscriptionSettings? _transcriptionSettings;
    private TranslationSettings? _translationSettings;

    #endregion

    public SubtitlesViewModel(
        ITranslationService translationService,
        LanguageService languageService,
        IPopupService popupService,
        SubtitlesMapper subtitlesMapper,
        ITranscriptionService transcriptionService,
        IBuiltInDialogService builtInDialogService
    )
    {
        #region observable properties

        MediaPath = null;
        Subtitles = [];
        Translations = [];

        #endregion

        _translationService = translationService;
        _popupService = popupService;
        _transcriptionService = transcriptionService;
        _builtInDialogService = builtInDialogService;
        _languageService = languageService;

        _subtitlesMapper = subtitlesMapper;
    }

    #region commands

    [RelayCommand]
    public void PositionChanged(TimeSpan currentPosition)
    {
        CurrentSubtitleIndex = FindNewIndex(currentPosition, Subtitles, CurrentSubtitleIndex);
        CurrentTranslationIndex = FindNewIndex(currentPosition, Translations, CurrentTranslationIndex);
    }

    [RelayCommand]
    public async Task Transcribe()
    {
        object? popupResult;

        if (_transcriptionSettings is null)
        {
            popupResult = await _popupService.ShowPopupAsync<TranscribePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.SubtitlesLanguage = _languageService.GetDefaultLanguage();
            });
        }
        else
        {
            popupResult = await _popupService.ShowPopupAsync<TranscribePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.SubtitlesLanguage = _transcriptionSettings.SubtitlesLanguage;
                vm.FromTime = _transcriptionSettings.FromTime;
                vm.ToTime = _transcriptionSettings.ToTime;
            });
        }

        if (popupResult is not TranscriptionSettings newSettings)
        {
            return;
        }

        _transcriptionSettings = newSettings;

        var results = _transcriptionService.TranscribeAsync(
            MediaPath,
            new TimeInterval(newSettings.FromTime, newSettings.ToTime),
            newSettings.SubtitlesLanguage.Code,
            default
        );

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                await _builtInDialogService.DisplayError(result.Error);

                return;
            }

            Subtitles.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(result.Value));
        }
    }

    [RelayCommand]
    public async Task Translate()
    {
        object? popupResult;

        if (_translationSettings is null)
        {
            popupResult = await _popupService.ShowPopupAsync<TranslatePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
            });
        }
        else
        {
            popupResult = await _popupService.ShowPopupAsync<TranslatePopupViewModel>(vm =>
            {
                vm.MediaDuration = MediaDuration;
                vm.TargetLanguage = _translationSettings.TargetLanguage;
                vm.FromTime = _translationSettings.FromTime;
                vm.ToTime = _translationSettings.ToTime;
            });
        }

        if (popupResult is not TranslationSettings newSettings)
        {
            return;
        }

        _translationSettings = newSettings;

        var subtitlesToTranslate = Subtitles.Where(s =>
            s.TimeInterval.StartTime >= newSettings.FromTime && s.TimeInterval.EndTime <= newSettings.ToTime
        );

        var subtitlesDtos = _subtitlesMapper.VisualSubtitlesToSubtitleDtoList(subtitlesToTranslate);

        var results = _translationService.TranslateAsync(subtitlesDtos, newSettings.TargetLanguage, default);

        await foreach (var result in results)
        {
            if (result.IsFailure)
            {
                await _builtInDialogService.DisplayError(result.Error);

                return;
            }

            Translations.Insert(_subtitlesMapper.SubtitleDtoToVisualSubtitle(result.Value));
        }
    }

    #endregion

    private static int FindNewIndex(TimeSpan currPosition, ObservableCollection<VisualSubtitle> subtitles, int currIndex)
    {
        if (subtitles is null or { Count: 0 })
        {
            return 0;
        }

        var currSub = subtitles[currIndex];

        if (currSub.TimeInterval.ContainsTime(currPosition))
        {
            return currIndex;
        }

        VisualSubtitle? prevSub = currIndex > 0 ? subtitles[currIndex - 1] : null;
        VisualSubtitle? nextSub = currIndex < subtitles.Count - 1 ? subtitles[currIndex + 1] : null;

        VisualSubtitle? newSub;
        int newIndex;

        if (prevSub is not null && prevSub.TimeInterval.ContainsTime(currPosition))
        {
            newSub = prevSub;
            newIndex = currIndex - 1;
        }
        else if (nextSub is not null && nextSub.TimeInterval.ContainsTime(currPosition))
        {
            newSub = nextSub;
            newIndex = currIndex + 1;
        }
        else
        {
            (newSub, newIndex) = subtitles.BinarySearch(currPosition);
        }

        if (newSub != null)
        {
            return newIndex;
        }

        return currIndex;
    }
}
