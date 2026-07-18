using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;

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

    public static List<Subtitle> ToSubtitleList(IEnumerable<VisualSubtitle> visualSubtitles)
    {
        return visualSubtitles.Select(ToSubtitle).ToList();
    }

    public static ObservableCollection<VisualSubtitle> ToVisualSubtitles(IEnumerable<Subtitle> subtitles)
    {
        return subtitles.Select(ToVisualSubtitle).ToObservableCollection();
    }
}
