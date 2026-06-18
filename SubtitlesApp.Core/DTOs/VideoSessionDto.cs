using System.ComponentModel.DataAnnotations;

namespace SubtitlesApp.Core.DTOs;

public class VideoSessionDto
{
    public string VideoId { get; set; }

    public TimeSpan PlaybackPosition { get; set; }

    public string SubtitlesReference { get; set; }

    public string TranslationsReference { get; set; }
}
