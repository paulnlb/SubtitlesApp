namespace SubtitlesServer.WhisperApi.Models;

public class WhisperDto
{
    public Stream AudioStream { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public int MaxSegmentLength { get; set; } = 0;
}
