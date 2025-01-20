namespace SubtitlesServer.WhisperApi.Interfaces;

public interface INlpService
{
    IAsyncEnumerable<string> SplitToSentences(string text, string languageCode);
}
