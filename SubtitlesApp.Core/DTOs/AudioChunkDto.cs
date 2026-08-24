namespace SubtitlesApp.Core.DTOs;

public class AudioChunkDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public Stream Audio { get; set; }
}
