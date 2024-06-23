namespace SubtitlesApp.Core.Models;

public class TrimmedAudioMetadata
{
    public int SampleRate { get; set; }

    public int ChannelsCount { get; set; }

    public string AudioFormat { get; set; }

    public TimeSpan StartTimeOffset { get; set; }

    public TimeSpan EndTime { get; set; }

    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Sets time offset regarding original audio (start time) and end time.
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="duration"></param>
    public void SetTimeBoundaries(TimeSpan startTime, TimeSpan duration)
    {
        StartTimeOffset = startTime;
        EndTime = startTime + duration;
        Duration = duration;
    }
}
