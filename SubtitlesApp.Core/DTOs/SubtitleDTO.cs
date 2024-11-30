using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.DTOs;

public class SubtitleDTO
{
    public string Text { get; set; }

    public TimeIntervalDTO TimeInterval { get; set; }

    public Language? Language { get; set; }
}
