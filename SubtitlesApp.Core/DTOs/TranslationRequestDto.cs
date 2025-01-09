namespace SubtitlesApp.Core.DTOs;

public class TranslationRequestDto
{
    public string TargetLanguageCode { get; set; }

    public List<SubtitleDto> SourceSubtitles { get; set; }
}
