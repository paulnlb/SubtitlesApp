using System.Collections.Generic;

namespace SubtitlesApp.Core.Models;

public class TimeSet
{
    protected List<TimeInterval> _timeIntervals = [];

    public virtual int Count { get => _timeIntervals.Count; }

    public virtual void Insert(TimeInterval newInterval)
    {
        (_, int index) = GetLaterClosestTo(newInterval.EndTime);

        if (index == -1)
        {
            index = _timeIntervals.Count;
        }

        var overlapsOrAdjacentLeft = index > 0 && (_timeIntervals[index - 1].Overlaps(newInterval) ||
            _timeIntervals[index - 1].IsAdjacentTo(newInterval));

        var overlapsOrAdjacentRight = index < _timeIntervals.Count && (_timeIntervals[index].Overlaps(newInterval) ||
            _timeIntervals[index].IsAdjacentTo(newInterval));

        if (overlapsOrAdjacentLeft && overlapsOrAdjacentRight)
        {
            _timeIntervals[index - 1] = _timeIntervals[index - 1].Union(_timeIntervals[index]);
            _timeIntervals.RemoveAt(index);
        }
        else if (overlapsOrAdjacentLeft)
        {
            _timeIntervals[index - 1] = _timeIntervals[index - 1].Union(newInterval);
        }
        else if (overlapsOrAdjacentRight)
        {
            _timeIntervals[index] = _timeIntervals[index].Union(newInterval);
        }
        else
        {
            _timeIntervals.Insert(index, newInterval);
        }
    }

    public virtual (TimeInterval? Interval, int index) GetByTimeStamp(TimeSpan timeStamp)
    {
        int low = 0;
        int high = _timeIntervals.Count - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            var midVal = _timeIntervals[mid];

            if (midVal.ContainsTime(timeStamp))
            {
                return (midVal, mid);
            }
            else if (midVal.IsEarlierThan(timeStamp))
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return (null, -1);
    }

    (TimeInterval? Sub, int index) GetLaterClosestTo(TimeSpan timeStamp)
    {
        int low = 0;
        int high = _timeIntervals.Count - 1;
        int mid = low + (high - low) / 2;

        if (_timeIntervals.Count == 0)
            return (null, 0);

        if (_timeIntervals[0].StartTime >= timeStamp)
            return (_timeIntervals[0], 0);
        if (_timeIntervals[^1].EndTime <= timeStamp)
            return (null, -1);

        while (low < high)
        {
            var midVal = _timeIntervals[mid];

            if (midVal.IsEarlierThan(timeStamp))
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }

            mid = low + (high - low) / 2;
        }

        return (_timeIntervals[mid], mid);
    }
}
