namespace SubtitlesServer.WhisperApi.Models;

public class WhisperRequestModel
{
    public IFormFile AudioFile { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    ///  Max subtitle length. Won't take effect if <see cref="OneSentencePerSubtitle"/> is set to true
    /// </summary>
    public int MaxSegmentLength { get; set; } = 0;

    /// <summary>
    /// When set to true, each subtitle is one sentence long, and <see cref="MaxSegmentLength"/> value is ignored.
    /// </summary>
    public bool OneSentencePerSubtitle { get; set; } = false;
}
