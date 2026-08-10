using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Services;

namespace SubtitlesApp.Mapper;

public static class SubtitlesMapper
{
    public static VisualSubtitle ToVisualSubtitle(Subtitle subtitle)
    {
        return new VisualSubtitle
        {
            Text = subtitle.Text,
            LanguageCode = subtitle.LanguageCode,
            TimeInterval = subtitle.TimeInterval,
            AdditionalInfo = subtitle.ToString(),
        };
    }

    public static Subtitle ToSubtitle(VisualSubtitle visualSubtitle)
    {
        return new Subtitle
        {
            Text = visualSubtitle.Text,
            LanguageCode = visualSubtitle.LanguageCode,
            TimeInterval = visualSubtitle.TimeInterval,
        };
    }

    public static Subtitle ToSubtitle(SrtItem srtItem)
    {
        return new Subtitle { Text = srtItem.Text, TimeInterval = new TimeInterval(srtItem.StartTime, srtItem.EndTime) };
    }

    public static SrtItem ToSrtItem(Subtitle subtitle, int index = 0)
    {
        return new SrtItem(index, subtitle.TimeInterval.StartTime, subtitle.TimeInterval.EndTime, subtitle.Text);
    }

    public static List<Subtitle> ToSubtitleList(IEnumerable<VisualSubtitle> visualSubtitles)
    {
        return visualSubtitles.Select(ToSubtitle).ToList();
    }

    public static ObservableCollection<VisualSubtitle> ToVisualSubtitles(IEnumerable<Subtitle> subtitles)
    {
        return subtitles.Select(ToVisualSubtitle).ToObservableCollection();
    }

    public static List<SrtItem> ToSrtItems(IEnumerable<Subtitle> subtitles)
    {
        var result = new List<SrtItem>();
        var i = 0;

        foreach (var subtitle in subtitles)
        {
            result.Add(ToSrtItem(subtitle, i));
            i++;
        }

        return result;
    }
}
