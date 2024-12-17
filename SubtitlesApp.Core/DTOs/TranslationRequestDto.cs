namespace SubtitlesApp.Core.DTOs;

public class TranslationRequestDto
{
    public string TargetLanguageCode { get; set; }

    public IEnumerable<SubtitleDTO> SourceSubtitles { get; set; }
}
