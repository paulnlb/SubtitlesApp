using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.CustomEventArgs;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.Repositories;

namespace SubtitlesApp.ViewModels;

public partial class PlayerWithSubtitlesViewModel : ObservableObject, IQueryAttributable
{
    #region private fields

    private readonly TimeSpan _positionRefreshTreshold = TimeSpan.FromSeconds(10);
    private VideoSessionDto? _session;
    private bool _shouldRefreshPosition;
    private readonly IVideoSessionRepository _videoSessionRepository;

    #endregion

    #region observable properties

    [ObservableProperty]
    private bool _playerControlsVisible;

    [ObservableProperty]
    private bool _isImmersiveOn;

    [ObservableProperty]
    private bool _isFullScreenOn;

    [ObservableProperty]
    private SubtitlesViewModel _subtitlesVm;

    [ObservableProperty]
    private MediaFileInfo? _fileInfo;

    #endregion

    public event EventHandler<SeekEventArgs>? SeekRequested;

    public PlayerWithSubtitlesViewModel(SubtitlesViewModel captionsViewModel, IVideoSessionRepository videoSessionRepository)
    {
        _videoSessionRepository = videoSessionRepository;
        PlayerControlsVisible = true;
        SubtitlesVm = captionsViewModel;
        StartRefreshingSession();
    }

    [RelayCommand]
    public async Task PositionChanged(TimeSpan currentPosition)
    {
        if (SubtitlesVm.UpdateIndexesCommand is not null && SubtitlesVm.UpdateIndexesCommand.CanExecute(currentPosition))
        {
            SubtitlesVm.UpdateIndexesCommand.Execute(currentPosition);
        }

        if (
            _shouldRefreshPosition
            && (
                currentPosition > _session!.PlaybackPosition + _positionRefreshTreshold
                || currentPosition < _session!.PlaybackPosition - _positionRefreshTreshold
            )
        )
        {
            _session.PlaybackPosition = currentPosition;

            await _videoSessionRepository.Update(_session);
        }
    }

    [RelayCommand]
    public void TogglePlayerControlsVisibility()
    {
        PlayerControlsVisible = !PlayerControlsVisible;
    }

    [RelayCommand]
    public async Task LoadSession()
    {
        if (FileInfo is null)
        {
            return;
        }

        _session = await _videoSessionRepository.Get(FileInfo.Id);

        if (_session is null)
        {
            _session = new VideoSessionDto(FileInfo.Id);

            SubtitlesVm.CachedSubtitlesFile = _session.SubtitlesReference;
            SubtitlesVm.CachedTranslationsFile = _session.TranslationsReference;

            await _videoSessionRepository.Create(_session);
            return;
        }

        if (_session.PlaybackPosition != TimeSpan.Zero)
        {
            SeekRequested?.Invoke(this, new SeekEventArgs { Time = _session.PlaybackPosition });
        }

        SubtitlesVm.CachedSubtitlesFile = _session.SubtitlesReference;
        SubtitlesVm.CachedTranslationsFile = _session.TranslationsReference;

        await SubtitlesVm.LoadSubtitlesFromCache();
        await SubtitlesVm.LoadTranslationsFromCache();
    }

    public void StartRefreshingSession()
    {
        _shouldRefreshPosition = true;
    }

    public void StopRefreshingSession()
    {
        _shouldRefreshPosition = false;
    }

    partial void OnIsFullScreenOnChanged(bool value)
    {
        IsImmersiveOn = value;
        PlayerControlsVisible = false;
    }

    partial void OnIsImmersiveOnChanged(bool value)
    {
        PlayerControlsVisible = false;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("open", out object? value))
        {
            if (value is MediaFileInfo fileInfo)
            {
                SubtitlesVm.FileInfo = FileInfo = fileInfo;
            }

            query.Clear();
        }
    }
}
