using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.DTOs;

public class SubtitleDTO
{
    public string Text { get; set; }

    public TimeIntervalDTO TimeInterval { get; set; }

    public string LanguageCode { get; set; }

    public bool IsTranslated { get; set; }

    public Translation? Translation { get; set; }
}
