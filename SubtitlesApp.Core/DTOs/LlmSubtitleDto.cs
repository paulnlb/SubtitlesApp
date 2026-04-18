using System;

namespace SubtitlesApp.Core.DTOs;

public class LlmSubtitleDto
{
    public string Text { get; set; }

    public double StartTime { get; set; }

    public double EndTime { get; set; }

    public string LanguageCode { get; set; }
}
