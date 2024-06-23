namespace SubtitlesAppServer.Models;

public class AudioMetadata
{
    public int SampleRate { get; set; }

    public int ChannelsCount { get; set; }

    public string AudioFormat { get; set; }

    public TimeSpan StartTimeOffset { get; set; }

    public TimeSpan Duration { get; set; }
}
