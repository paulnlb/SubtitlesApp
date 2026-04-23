namespace SubtitlesApp.Core.Models;

public class Subtitle
{
    private string Text { get; set; }
    private string LanguageCode { get; set; }
    public TimeInterval TimeInterval { get; set; }
}
