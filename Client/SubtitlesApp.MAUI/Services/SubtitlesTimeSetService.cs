using SubtitlesApp.Core.Models;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class SubtitlesTimeSetService : ISubtitlesTimeSetService
{
    public TimeInterval? GetTimeIntervalForTranscription(
        TimeSet coveredTimeIntervals, 
        TimeSpan currentPosition,
        TimeSpan transcribeBufferLength,
        TimeSpan mediaDuration)
    {
        (var currentInterval, _) = coveredTimeIntervals.GetByTimeStamp(currentPosition);

        var startTime = currentInterval == null ? currentPosition : currentInterval.EndTime;

        if (startTime >= mediaDuration)
        {
            return null;
        }

        if (startTime <= TimeSpan.FromSeconds(1))
        {
            // Start from the beginning
            startTime = TimeSpan.Zero;
        }

        var endTime = startTime.Add(transcribeBufferLength);

        if (endTime > mediaDuration)
        {
            endTime = mediaDuration;
        }

        return new TimeInterval(startTime, endTime);
    }

    public bool ShouldStartTranscription(
        TimeSet coveredTimeIntervals,
        TimeSpan currentPosition,
        TimeSpan mediaDuration)
    {
        (var currentInterval, _) = coveredTimeIntervals.GetByTimeStamp(currentPosition);

        // If the current interval is the last one and it covers the end of the media
        // return false
        if (currentInterval != null && currentInterval.EndTime >= mediaDuration)
        {
            return false;
        }

        var isTimeSuitableForTranscribe =
            currentInterval == null ||
            currentInterval.EndTime - currentPosition <= TimeSpan.FromSeconds(15);

        return isTimeSuitableForTranscribe;
    }
}
