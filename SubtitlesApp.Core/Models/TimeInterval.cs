namespace SubtitlesApp.Core.Models;

public class TimeInterval
{
    public TimeInterval()
    {
        StartTime = TimeSpan.Zero;
        EndTime = TimeSpan.Zero;
    }

    public TimeInterval(TimeSpan startTime, TimeSpan endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public TimeSpan StartTime { get; }

    public TimeSpan EndTime { get; }

    public bool ContainsTime(TimeSpan time) =>
        StartTime <= time && EndTime > time;

    public bool IsEarlierThan(TimeSpan time) =>
        StartTime < time && EndTime < time;

    public bool IsLaterThan(TimeSpan time) =>
        StartTime > time && EndTime > time;

    public bool Overlaps(TimeInterval other)
    {
        return EndTime > other.StartTime && StartTime < other.EndTime;
    }

    public bool IsAdjacentTo(TimeInterval other)
    {
        return EndTime == other.StartTime || StartTime == other.EndTime;
    }

    public TimeInterval Union(TimeInterval other)
    {
        return new TimeInterval(
            StartTime < other.StartTime ? StartTime : other.StartTime,
            EndTime > other.EndTime ? EndTime : other.EndTime);
    }

    public TimeInterval Substract(TimeInterval other)
    {
        if (!Overlaps(other))
        {
            return this;
        }

        if (StartTime < other.StartTime)
        {
            return new TimeInterval(StartTime, other.StartTime);
        }

        return new TimeInterval(other.EndTime, EndTime);
    }
}
