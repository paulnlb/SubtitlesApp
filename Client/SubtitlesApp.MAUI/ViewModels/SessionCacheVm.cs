using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Constants;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.Repositories;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels;

public partial class SessionCacheVm : ObservableObject
{
    [ObservableProperty]
    private List<PickerItem> _clearOptions;

    [ObservableProperty]
    private PickerItem? _selectedItem;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private bool _isCustom;

    private readonly IVideoSessionRepository _videoSessionRepository;
    private readonly IBuiltInDialogService _builtInDialogService;
    private readonly ISubtitlesCache _subtitlesCache;

    public SessionCacheVm(
        IVideoSessionRepository videoSessionRepository,
        IBuiltInDialogService builtInDialogService,
        ISubtitlesCache subtitlesCache
    )
    {
        ClearOptions =
        [
            new() { Title = "Last 1 day", Action = TimePeriods.Day },
            new() { Title = "Last 1 week", Action = TimePeriods.Week },
            new() { Title = "Last 4 weeks", Action = TimePeriods.FourWeeks },
            new() { Title = "All time", Action = TimePeriods.AllTime },
            new() { Title = "Custom", Action = TimePeriods.Custom },
        ];

        _videoSessionRepository = videoSessionRepository;
        _builtInDialogService = builtInDialogService;
        _subtitlesCache = subtitlesCache;
    }

    [RelayCommand]
    public async Task ClearCache()
    {
        DateTimeOffset? utcStartDate = StartDate is null ? null : new DateTimeOffset(StartDate.Value).ToUniversalTime();
        DateTimeOffset? utcEndDate = EndDate is null ? null : new DateTimeOffset(EndDate.Value).ToUniversalTime();

        var videoSessions = await _videoSessionRepository.GetMany(utcStartDate, utcEndDate);

        if (videoSessions.Count == 0)
        {
            await _builtInDialogService.DisplayAlert(
                "No Cached Video Sessions",
                "There are no cached video sessions that match selected criteria",
                "Ok"
            );

            return;
        }

        var shouldDelete = await _builtInDialogService.DisplayAlert(
            "Clear Cache",
            $"{videoSessions.Count} items will be deleted. Are you sure?",
            "Yes",
            "Cancel"
        );

        if (!shouldDelete)
        {
            return;
        }

        var videoIds = new List<string>();

        foreach (var session in videoSessions)
        {
            if (!string.IsNullOrWhiteSpace(session.SubtitlesReference))
            {
                _subtitlesCache.Delete(session.SubtitlesReference);
            }

            if (!string.IsNullOrWhiteSpace(session.TranslationsReference))
            {
                _subtitlesCache.Delete(session.TranslationsReference);
            }

            videoIds.Add(session.VideoId);
        }

        await _videoSessionRepository.DeleteMany(videoIds);

        await _builtInDialogService.DisplayAlert("Success", "Cache Cleared Successfully", "Ok");
    }

    partial void OnSelectedItemChanged(PickerItem? value)
    {
        if (value is null)
        {
            return;
        }

        IsCustom = value.Action == TimePeriods.Custom;

        switch (value.Action)
        {
            case TimePeriods.Day:
                StartDate = DateTime.Now.AddDays(-1);
                EndDate = null;
                break;
            case TimePeriods.Week:
                StartDate = DateTime.Now.AddDays(-7);
                EndDate = null;
                break;
            case TimePeriods.FourWeeks:
                StartDate = DateTime.Now.AddDays(-28);
                EndDate = null;
                break;
            case TimePeriods.Custom:
                StartDate = EndDate = null;
                break;
            case TimePeriods.AllTime:
                StartDate = DateTime.MinValue;
                EndDate = DateTime.MaxValue;
                break;
        }
    }
}
