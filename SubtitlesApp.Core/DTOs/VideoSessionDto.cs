using System.Text;

namespace SubtitlesApp.Core.DTOs;

public class VideoSessionDto
{
    public VideoSessionDto() { }

    public VideoSessionDto(string videoSessionId)
    {
        VideoId = videoSessionId;
        SubtitlesReference = GenerateSubtitlesReference(videoSessionId);
        TranslationsReference = GenerateTranslationsReference(videoSessionId);
    }

    public string VideoId { get; set; }

    public TimeSpan PlaybackPosition { get; set; }

    public string SubtitlesReference { get; set; }

    public string TranslationsReference { get; set; }

    private static string GenerateSubtitlesReference(string videoId)
    {
        var key = GenerateSubtitlesKey(videoId);
        return $"subtiltes-{key}";
    }

    private static string GenerateTranslationsReference(string videoId)
    {
        var key = GenerateSubtitlesKey(videoId);
        return $"translation-{key}";
    }

    private static string GenerateSubtitlesKey(string videoId)
    {
        var fileName = Path.GetFileNameWithoutExtension(videoId);

        var keyBuilder = new StringBuilder();

        // remove spaces and special characters
        foreach (char c in fileName)
        {
            if (char.IsLetterOrDigit(c))
            {
                keyBuilder.Append(c);
            }
        }

        keyBuilder.Append(DateTime.Now.ToString("yyyyMMddhhmmss"));

        return keyBuilder.ToString();
    }
}
