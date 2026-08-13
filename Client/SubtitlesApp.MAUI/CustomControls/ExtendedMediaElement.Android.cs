using Android.Widget;
using AndroidX.Media3.Common;
using AndroidX.Media3.UI;
using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;

namespace SubtitlesApp.CustomControls;

public partial class ExtendedMediaElement : MediaElement
{
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

        var trackNo = 1;
        var isSelected = false;

        foreach (Tracks.Group group in currentGroups)
        {
            if (group.Type != C.TrackTypeAudio)
            {
                continue;
            }

            for (int i = 0; i < group.Length; i++)
            {
                if (trackNo == selectedTrackNo)
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
                    trackNo++;
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

    public partial List<MediaTrack> GetAudioTracks()
    {
        var trackList = new List<MediaTrack>();

        if (Handler is not MediaElementHandler mediaElementHandler)
            return [];

        var player = GetPlayer(mediaElementHandler.PlatformView);

        if (player is null)
            return [];

        var currentGroups = player.CurrentTracks?.Groups;

        if (currentGroups is null)
        {
            return [];
        }

        var trackNo = 1;

        foreach (Tracks.Group group in currentGroups)
        {
            if (group.Type != C.TrackTypeAudio)
            {
                continue;
            }

            for (int i = 0; i < group.Length; i++)
            {
                var format = group.GetTrackFormat(i);

                var name = format.Label ?? $"Audio Track {trackNo}";
                name += $" - {format.Language}";

                trackList.Add(
                    new MediaTrack()
                    {
                        TrackNo = trackNo,
                        Name = name,
                        IsSelected = group.IsTrackSelected(i),
                    }
                );

                trackNo++;
            }
        }

        return trackList;
    }

    public partial List<MediaTrack> GetSubtitleTracks()
    {
        var trackList = new List<MediaTrack>();

        if (Handler is not MediaElementHandler mediaElementHandler)
            return [];

        var player = GetPlayer(mediaElementHandler.PlatformView);

        if (player is null)
            return [];

        var currentGroups = player.CurrentTracks?.Groups;

        if (currentGroups is null)
        {
            return [];
        }

        var trackNo = 1;

        foreach (Tracks.Group group in currentGroups)
        {
            if (group.Type != C.TrackTypeText)
            {
                continue;
            }

            for (int i = 0; i < group.Length; i++)
            {
                var format = group.GetTrackFormat(i);

                var name = format.Label ?? $"Subtitle Track {trackNo}";
                name += $" - {format.Language}";

                trackList.Add(
                    new MediaTrack()
                    {
                        TrackNo = trackNo,
                        Name = name,
                        IsSelected = group.IsTrackSelected(i),
                    }
                );

                trackNo++;
            }
        }

        return trackList;
    }

    private IPlayer? GetPlayer(MauiMediaElement mauiMediaElement)
    {
        var relativeLayout = mauiMediaElement.GetChildAt(0) as RelativeLayout;

        var playerView = relativeLayout?.GetChildAt(0) as PlayerView;

        return playerView?.Player;
    }
}
