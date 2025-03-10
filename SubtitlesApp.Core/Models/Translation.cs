namespace SubtitlesApp.Core.Models;

public class Translation
{
    public required string LanguageCode { get; set; } = default!;

    public required string Text { get; set; } = default!;
}
