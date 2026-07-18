using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.Mapper;

public static class SubtitleMapper
{
    public static Subtitle ToDomainClass(SubtitleSerializable entity)
    {
        return new Subtitle
        {
            Text = entity.Text,
            LanguageCode = entity.LanguageCode,
            TimeInterval = new TimeInterval(entity.StartTime, entity.EndTime),
        };
    }

    public static SubtitleSerializable ToSerializable(Subtitle subtitle)
    {
        return new SubtitleSerializable
        {
            Text = subtitle.Text,
            LanguageCode = subtitle.LanguageCode,
            StartTime = subtitle.TimeInterval.StartTime,
            EndTime = subtitle.TimeInterval.EndTime,
        };
    }

    public static IEnumerable<Subtitle> ToDomainClasses(IEnumerable<SubtitleSerializable> entities)
    {
        return entities.Select(ToDomainClass);
    }

    public static IEnumerable<SubtitleSerializable> ToSerializables(IEnumerable<Subtitle> subtitles)
    {
        return subtitles.Select(ToSerializable);
    }
}
