namespace SubtitlesApp.Core.DTOs;

public class TranslationRequestDto
{
    public string TargetLanguageCode { get; set; }

    public List<SubtitleDTO> SourceSubtitles { get; set; }
}
