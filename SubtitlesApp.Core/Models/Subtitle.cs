namespace SubtitlesApp.Core.Models;

public class Subtitle
{
    public string Text { get; set; }
    public string LanguageCode { get; set; }
    public TimeInterval TimeInterval { get; set; }

    public override string ToString()
    {
        return $"""
            Start Time: {TimeInterval.StartTime};
            End Time: {TimeInterval.EndTime};
            Text: {Text}
            """;
    }
}
