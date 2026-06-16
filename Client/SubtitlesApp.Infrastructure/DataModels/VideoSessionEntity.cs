using SQLite;

namespace SubtitlesApp.Infrastructure.DataModels;

public class VideoSessionEntity
{
    [PrimaryKey]
    public string VideoId { get; set; }

    public TimeSpan PlaybackPosition { get; set; }

    public string SubtitlesReference { get; set; }

    public string TranslationsReference { get; set; }
}
