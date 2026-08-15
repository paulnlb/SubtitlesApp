using System.Collections.ObjectModel;
using Android.Widget;
using AndroidX.Media3.Common;
using AndroidX.Media3.Common.Text;
using AndroidX.Media3.UI;
using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;

namespace SubtitlesApp.CustomControls;

public partial class ExtendedMediaElement : MediaElement
{
    private bool _disposed = false;
    private PlayerListener? _playerListener;

    public static readonly BindablePropertyKey AudioTracksPropertyKey = BindableProperty.CreateReadOnly(
        nameof(AudioTracks),
        typeof(ReadOnlyCollection<MediaTrack>),
        typeof(ExtendedMediaElement),
        new List<MediaTrack>().AsReadOnly()
    );

    public static readonly BindableProperty AudioTracksProperty = AudioTracksPropertyKey.BindableProperty;

    public static readonly BindablePropertyKey SubtitleTracksPropertyKey = BindableProperty.CreateReadOnly(
        nameof(SubtitleTracks),
        typeof(ReadOnlyCollection<MediaTrack>),
        typeof(ExtendedMediaElement),
        new List<MediaTrack>().AsReadOnly()
    );

    public static readonly BindableProperty SubtitleTracksProperty = SubtitleTracksPropertyKey.BindableProperty;

    public ReadOnlyCollection<MediaTrack> AudioTracks
    {
        get => (ReadOnlyCollection<MediaTrack>)GetValue(AudioTracksProperty);
        private set => SetValue(AudioTracksPropertyKey, value);
    }

    public ReadOnlyCollection<MediaTrack> SubtitleTracks
    {
        get => (ReadOnlyCollection<MediaTrack>)GetValue(SubtitleTracksProperty);
        private set => SetValue(SubtitleTracksPropertyKey, value);
    }

    public partial void SelectAudioTrack(int selectedTrackNo)
    {
        if (Handler is not MediaElementHandler mediaElementHandler)
            return;

        var player = GetPlayer(mediaElementHandler.PlatformView);

        if (player is null)
            return;

        var currentGroups = player.CurrentTracks?.Groups;

        if (currentGroups is null)
            return;

        var trackIndex = 0;
        var isSelected = false;

        foreach (Tracks.Group group in currentGroups)
        {
            if (group.Type != C.TrackTypeAudio)
            {
                continue;
            }

            for (int i = 0; i < group.Length; i++)
            {
                if (trackIndex == selectedTrackNo)
                {
                    var trackOverride = new TrackSelectionOverride(group.MediaTrackGroup, i);

                    player.TrackSelectionParameters = player
                        .TrackSelectionParameters.BuildUpon()
                        .ClearOverridesOfType(C.TrackTypeAudio)
                        .AddOverride(trackOverride)
                        .Build();

                    isSelected = true;
                }
                else
                {
                    trackIndex++;
                    continue;
                }

                if (isSelected)
                {
                    break;
                }
            }

            if (isSelected)
            {
                break;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            if (_playerListener is not null && Handler is MediaElementHandler mediaElementHandler)
            {
                var player = GetPlayer(mediaElementHandler.PlatformView);

                player?.RemoveListener(_playerListener);

                _playerListener = null;
            }
        }

        base.Dispose(disposing);

        _disposed = true;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not MediaElementHandler mediaElementHandler)
        {
            return;
        }

        var player = GetPlayer(mediaElementHandler.PlatformView);

        if (player is null)
        {
            return;
        }

        _playerListener = new PlayerListener(
            audioTracks => AudioTracks = audioTracks,
            subtitleTracks => SubtitleTracks = subtitleTracks
        );

        player.AddListener(_playerListener);
    }

    private IPlayer? GetPlayer(MauiMediaElement mauiMediaElement)
    {
        var relativeLayout = mauiMediaElement.GetChildAt(0) as RelativeLayout;

        var playerView = relativeLayout?.GetChildAt(0) as PlayerView;

        return playerView?.Player;
    }
}

public class PlayerListener(
    Action<ReadOnlyCollection<MediaTrack>> audioTracksSetter,
    Action<ReadOnlyCollection<MediaTrack>> subtitlesTracksSetter
) : Java.Lang.Object, IPlayerListener
{
    public void OnTracksChanged(Tracks? tracks)
    {
        if (tracks is null)
        {
            return;
        }

        var audioTracks = GetAudioTracks(tracks).AsReadOnly();
        var subtitleTracks = GetSubtitleTracks(tracks).AsReadOnly();

        audioTracksSetter(audioTracks);
        subtitlesTracksSetter(subtitleTracks);
    }

    #region PlayerListener implementation method stubs

    public void OnPlaybackParametersChanged(PlaybackParameters? playbackParameters) { }

    public void OnPlayerStateChanged(bool playWhenReady, int playbackState) { }

    public void OnPlaybackStateChanged(int playbackState) { }

    public void OnPlayerError(PlaybackException? error) { }

    public void OnVideoSizeChanged(VideoSize? videoSize) { }

    public void OnVolumeChanged(float volume) { }

    public void OnAudioAttributesChanged(AudioAttributes? audioAttributes) { }

    public void OnAudioSessionIdChanged(int audioSessionId) { }

    public void OnAvailableCommandsChanged(PlayerCommands? player) { }

    public void OnCues(CueGroup? cues) { }

    public void OnDeviceInfoChanged(AndroidX.Media3.Common.DeviceInfo? deviceInfo) { }

    public void OnDeviceVolumeChanged(int volume, bool muted) { }

    public void OnEvents(IPlayer? player, PlayerEvents? playerEvents) { }

    public void OnIsLoadingChanged(bool isLoading) { }

    public void OnIsPlayingChanged(bool isPlaying) { }

    public void OnLoadingChanged(bool isLoading) { }

    public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs) { }

    public void OnMediaItemTransition(MediaItem? mediaItem, int reason) { }

    public void OnMediaMetadataChanged(MediaMetadata? mediaMetadata) { }

    public void OnMetadata(Metadata? metadata) { }

    public void OnPlayWhenReadyChanged(bool playWhenReady, int reason) { }

    public void OnPositionDiscontinuity(PlayerPositionInfo? oldPosition, PlayerPositionInfo? newPosition, int reason) { }

    public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason) { }

    public void OnPlayerErrorChanged(PlaybackException? error) { }

    public void OnPlaylistMetadataChanged(MediaMetadata? mediaMetadata) { }

    public void OnRenderedFirstFrame() { }

    public void OnRepeatModeChanged(int repeatMode) { }

    public void OnSeekBackIncrementChanged(long seekBackIncrementMs) { }

    public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs) { }

    public void OnShuffleModeEnabledChanged(bool shuffleModeEnabled) { }

    public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled) { }

    public void OnSurfaceSizeChanged(int width, int height) { }

    public void OnTimelineChanged(Timeline? timeline, int reason) { }

    public void OnTrackSelectionParametersChanged(TrackSelectionParameters? trackSelectionParameters) { }
    #endregion

    private List<MediaTrack> GetAudioTracks(Tracks tracks)
    {
        var trackList = new List<MediaTrack>();

        var currentGroups = tracks.Groups;

        if (currentGroups is null)
        {
            return [];
        }

        var trackIndex = 0;

        foreach (Tracks.Group group in currentGroups)
        {
            if (group.Type != C.TrackTypeAudio)
            {
                continue;
            }

            for (int i = 0; i < group.Length; i++)
            {
                var format = group.GetTrackFormat(i);

                var name = format.Label ?? $"Audio Track {trackIndex + 1}";
                name += $" - {format.Language}";

                trackList.Add(
                    new MediaTrack()
                    {
                        TrackIndex = trackIndex,
                        Name = name,
                        IsSelected = group.IsTrackSelected(i),
                    }
                );

                trackIndex++;
            }
        }

        return trackList;
    }

    private List<MediaTrack> GetSubtitleTracks(Tracks tracks)
    {
        var trackList = new List<MediaTrack>();

        var currentGroups = tracks.Groups;

        if (currentGroups is null)
        {
            return [];
        }

        var trackIndex = 0;

        foreach (Tracks.Group group in currentGroups)
        {
            if (group.Type != C.TrackTypeText)
            {
                continue;
            }

            for (int i = 0; i < group.Length; i++)
            {
                var format = group.GetTrackFormat(i);

                var name = format.Label ?? $"Subtitle Track {trackIndex + 1}";
                name += $" - {format.Language}";

                trackList.Add(
                    new MediaTrack()
                    {
                        TrackIndex = trackIndex,
                        Name = name,
                        IsSelected = group.IsTrackSelected(i),
                    }
                );

                trackIndex++;
            }
        }

        return trackList;
    }
}
