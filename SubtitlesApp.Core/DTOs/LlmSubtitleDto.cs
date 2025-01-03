namespace SubtitlesApp.Core.DTOs;

public class LlmSubtitleDto
{
    public string Text { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string LanguageCode { get; set; }
}
