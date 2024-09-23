namespace SubtitlesApp.Core.DTOs;

public class TrimmedAudioMetadataDTO
{
    public int SampleRate { get; set; }

    public int ChannelsCount { get; set; }

    public string AudioFormat { get; set; }

    public TimeSpan StartTimeOffset { get; set; }

    public TimeSpan EndTime { get; set; }
}
