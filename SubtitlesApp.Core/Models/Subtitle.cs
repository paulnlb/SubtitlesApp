namespace SubtitlesApp.Core.Models;

public class Subtitle
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Text { get; set; }
}
