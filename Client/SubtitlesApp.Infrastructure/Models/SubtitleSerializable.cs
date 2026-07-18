using MessagePack;

namespace SubtitlesApp.Infrastructure.Models;

[MessagePackObject]
public class SubtitleSerializable
{
    [Key(0)]
    public string Text { get; set; }

    [Key(1)]
    public TimeSpan StartTime { get; set; }

    [Key(2)]
    public TimeSpan EndTime { get; set; }

    [Key(3)]
    public string LanguageCode { get; set; }
}
