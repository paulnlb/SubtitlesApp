using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels.CustomEventArgs;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.Repositories;
using SubtitlesApp.Infrastructure.Interfaces;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Mapper;

namespace SubtitlesApp.ViewModels;

public partial class PlayerWithSubtitlesViewModel : ObservableObject, IQueryAttributable
{
    private readonly IVideoSessionRepository _videoSessionRepository;
    private readonly ISubtitlesRepository _subtitlesRepository;

    private readonly IBuiltInDialogService _builtInDialogService;
    private readonly ICustomFilePicker _filePicker;

    #region private fields

    private readonly TimeSpan _positionRefreshTreshold = TimeSpan.FromSeconds(10);
    private VideoSessionDto? _session;
    private bool _shouldRefreshPosition;

    #endregion

    #region observable properties

    [ObservableProperty]
    private string? _mediaPath;

    [ObservableProperty]
    private bool _playerControlsVisible;

    [ObservableProperty]
    private bool _isImmersiveOn;

    [ObservableProperty]
    private bool _isFullScreenOn;

    [ObservableProperty]
    private SubtitlesViewModel _subtitlesVm;

    [ObservableProperty]
    private Stream? _mediaStream;

    [ObservableProperty]
    private string _title = string.Empty;

    #endregion

    public event EventHandler<SeekEventArgs>? SeekRequested;

    public PlayerWithSubtitlesViewModel(
        SubtitlesViewModel captionsViewModel,
        IVideoSessionRepository videoSessionRepository,
        ISubtitlesRepository subtitlesRepository,
        IBuiltInDialogService builtInDialogService,
        ICustomFilePicker filePicker
    )
    {
        _videoSessionRepository = videoSessionRepository;
        _subtitlesRepository = subtitlesRepository;
        _filePicker = filePicker;
        _builtInDialogService = builtInDialogService;
        PlayerControlsVisible = true;
        MediaPath = null;
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
        _session = await _videoSessionRepository.Get(MediaPath);

        if (_session is null)
        {
            _session = new VideoSessionDto { VideoId = MediaPath };
            await _videoSessionRepository.Create(_session);
            return;
        }

        if (_session.PlaybackPosition != TimeSpan.Zero)
        {
            SeekRequested?.Invoke(this, new SeekEventArgs { Time = _session.PlaybackPosition });
        }

        SubtitlesVm.IsTranscriptionLoading = true;
        SubtitlesVm.IsTranslationLoading = true;

        try
        {
            if (!string.IsNullOrWhiteSpace(_session.SubtitlesReference))
            {
                SubtitlesVm.Subtitles = SubtitlesMapper.ToVisualSubtitles(
                    await _subtitlesRepository.Get(_session.SubtitlesReference)
                );
            }

            if (!string.IsNullOrWhiteSpace(_session.TranslationsReference))
            {
                SubtitlesVm.Translations = SubtitlesMapper.ToVisualSubtitles(
                    await _subtitlesRepository.Get(_session.TranslationsReference)
                );
            }
        }
        finally
        {
            SubtitlesVm.IsTranscriptionLoading = false;
            SubtitlesVm.IsTranslationLoading = false;
        }
    }

    public void StartRefreshingSession()
    {
        _shouldRefreshPosition = true;
        SubtitlesVm.SubtitlesUpdated += OnSubtitlesUpdated;
        SubtitlesVm.TranslationsUpdated += OnTranslationsUpdated;
    }

    public void StopRefreshingSession()
    {
        _shouldRefreshPosition = false;
        SubtitlesVm.SubtitlesUpdated -= OnSubtitlesUpdated;
        SubtitlesVm.TranslationsUpdated -= OnTranslationsUpdated;
    }

    private async Task OnSubtitlesUpdated()
    {
        if (SubtitlesVm.Subtitles.Count == 0 || _session is null)
        {
            return;
        }

        var key = GenerateSubtitlesKey(_session.VideoId);
        var oldReference = _session.SubtitlesReference;
        var newReference = $"subtiltes-{key}";

        await _subtitlesRepository.Create(newReference, SubtitlesVm.Subtitles);

        _session.SubtitlesReference = newReference;

        if (!string.IsNullOrWhiteSpace(oldReference))
        {
            _subtitlesRepository.Delete(oldReference);
        }

        await _videoSessionRepository.Update(_session);
    }

    private async Task OnTranslationsUpdated()
    {
        if (SubtitlesVm.Translations.Count == 0 || _session is null)
        {
            return;
        }

        var key = GenerateSubtitlesKey(_session.VideoId);
        var oldReference = _session.TranslationsReference;
        var newReference = $"translation-{key}";

        await _subtitlesRepository.Create(newReference, SubtitlesVm.Translations);

        _session.TranslationsReference = newReference;

        if (!string.IsNullOrWhiteSpace(oldReference))
        {
            _subtitlesRepository.Delete(oldReference);
        }

        await _videoSessionRepository.Update(_session);
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
            var uri = value.ToString()!;
            query.Clear();

            var streamResult = _filePicker.GetFileStream(uri);

            if (streamResult.IsFailure)
            {
                _builtInDialogService.DisplayError(streamResult.Error);
                return;
            }

            MediaStream = streamResult.Value;
            SubtitlesVm.MediaPath = MediaPath = uri;

            var fileNameResult = _filePicker.GetFileName(uri);

            if (fileNameResult.IsFailure)
            {
                _builtInDialogService.DisplayError(fileNameResult.Error);
                return;
            }

            Title = fileNameResult.Value;
        }
    }

    private static string GenerateSubtitlesKey(string videoId)
    {
        var fileName = Path.GetFileNameWithoutExtension(videoId);

        var keyBuilder = new StringBuilder();

        // remove spaces and special characters
        foreach (char c in fileName)
        {
            if (char.IsLetterOrDigit(c))
            {
                keyBuilder.Append(c);
            }
        }

        keyBuilder.Append(DateTime.Now.ToString("yyyyMMddhhmmss"));

        return keyBuilder.ToString();
    }
}
