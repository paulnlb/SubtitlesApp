namespace SubtitlesServer.Infrastructure.Configs;

public class OllamaConfig
{
    public string BaseUrl { get; set; }

    public string ModelName { get; set; }

    public float Temperature { get; set; }

    public int NumCtx { get; set; }

    public string DefaultSystemPrompt { get; set; }

    public string SingleSubtitleSystemPrompt { get; set; }
}
