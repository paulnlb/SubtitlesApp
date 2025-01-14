namespace SubtitlesServer.WhisperApi.Configs;

public class RateLimiterConfig
{
    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }
}
