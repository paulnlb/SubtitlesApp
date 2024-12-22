using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Interfaces;

public interface ISubtitlesTimeSetService
{
    TimeInterval? GetTimeIntervalForTranscription(
        TimeSet coveredTimeIntervals,
        TimeSpan currentPosition,
        TimeSpan transcribeBufferLength,
        TimeSpan mediaDuration);

    bool ShouldStartTranscription(
        TimeSet coveredTimeIntervals,
        TimeSpan currentPosition,
        TimeSpan mediaDuration);


}
