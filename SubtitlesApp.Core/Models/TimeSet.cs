namespace SubtitlesApp.Core.Models;

/// <summary>
///     Represents a set of time intervals. The intervals are stored in LinkedList and are sorted by their start time.
/// </summary>
public class TimeSet
{
    private readonly LinkedList<TimeInterval> _timeIntervals = [];

    private TimeInterval? _cachedTimeInterval;

    public TimeSet() { }

    public TimeSet(IEnumerable<TimeInterval> timeIntervals, TimeSpan mergeThreshold = default)
    {
        foreach (var interval in timeIntervals)
        {
            Insert(interval, mergeThreshold);
        }
    }

    public int Count
    {
        get => _timeIntervals.Count;
    }

    public TimeInterval? GetByTimeStamp(TimeSpan timeStamp)
    {
        if (_cachedTimeInterval?.ContainsTime(timeStamp) == true)
        {
            return _cachedTimeInterval;
        }

        var currentNode = _timeIntervals.First;

        while (currentNode != null)
        {
            if (currentNode.Value.ContainsTime(timeStamp))
            {
                _cachedTimeInterval = currentNode.Value;

                return currentNode.Value;
            }

            currentNode = currentNode.Next;
        }

        return null;
    }

    /// <summary>
    ///     This method iterates through the list of time intervals and does the following:<br/>
    ///     - removes all intervals that overlap newInterval;<br/>
    ///     - unites newInterval with all intervals that overlap it;<br/>
    ///     - finds the nearest interval that is earlier than newInterval and inserts newInterval right after it.
    /// </summary>
    /// <param name="newInterval">Time interval to insert.</param>
    /// <param name="mergeThreshold">
    ///     If time distance between <param name="newInterval"> and any existing time interval is less than
    ///     or equal to <param name="mergeThreshold">, <param name="newInterval"> will be merged into
    ///     its neighboring time interval instead of being inserted.
    ///     If not specified, <param name="newInterval"> is only merged into a neighbor when they overlap or are adjacent.
    /// </param>
    /// <returns></returns>
    public void Insert(TimeInterval newInterval, TimeSpan mergeThreshold = default)
    {
        var currentNode = _timeIntervals.First;
        LinkedListNode<TimeInterval>? nodeToInsertAfter = null;

        while (currentNode != null)
        {
            var currentInterval = currentNode.Value;

            if (currentInterval.Overlaps(newInterval) || currentInterval.IsNearTo(newInterval, mergeThreshold))
            {
                newInterval = newInterval.Union(currentInterval);

                var nextNode = currentNode.Next;
                _timeIntervals.Remove(currentNode);
                currentNode = nextNode;

                continue;
            }
            else if (currentInterval.IsEarlierThan(newInterval.StartTime))
            {
                nodeToInsertAfter = currentNode;
            }
            else if (currentInterval.IsLaterThan(newInterval.EndTime))
            {
                break;
            }

            currentNode = currentNode.Next;
        }

        if (nodeToInsertAfter?.List == null)
        {
            _timeIntervals.AddFirst(newInterval);
        }
        else
        {
            _timeIntervals.AddAfter(nodeToInsertAfter, newInterval);
        }

        _cachedTimeInterval = null;
    }

    public IEnumerable<TimeInterval> GetAllIntervals() => _timeIntervals;

    public IEnumerable<TimeInterval> GetAllGaps()
    {
        if (_timeIntervals.Count == 0)
        {
            return [];
        }

        return GetAllGapsInside(_timeIntervals.First!.Value.StartTime, _timeIntervals.Last!.Value.EndTime);
    }

    public IEnumerable<TimeInterval> GetAllGapsInside(TimeSpan startTime, TimeSpan endTime)
    {
        if (_timeIntervals.Count == 0 || startTime == endTime)
        {
            yield break;
        }

        var lastEnd = startTime;

        foreach (var interval in _timeIntervals)
        {
            if (interval.StartTime > lastEnd)
            {
                yield return new TimeInterval(lastEnd, interval.StartTime);
            }

            lastEnd = interval.EndTime;
        }

        if (lastEnd < endTime)
        {
            yield return new TimeInterval(lastEnd, endTime);
        }
    }
}
