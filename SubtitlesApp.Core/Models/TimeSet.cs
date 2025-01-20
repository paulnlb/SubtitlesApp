namespace SubtitlesApp.Core.Models;

/// <summary>
///     Represents a set of time intervals. The intervals are stored in LinkedList and are sorted by their start time.
/// </summary>
public class TimeSet
{
    private readonly LinkedList<TimeInterval> _timeIntervals = [];

    private TimeInterval? _cachedTimeInterval;

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
    /// <param name="newInterval"></param>
    /// <returns></returns>
    public void Insert(TimeInterval newInterval)
    {
        var currentNode = _timeIntervals.First;
        LinkedListNode<TimeInterval>? nodeToInsertAfter = null;

        while (currentNode != null)
        {
            var currentInterval = currentNode.Value;

            if (currentInterval.OverlapsOrAdjacentTo(newInterval))
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
}
