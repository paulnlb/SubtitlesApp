using SubtitlesApp.Core.Models;
using System.Collections.ObjectModel;

namespace SubtitlesApp.Shared.Extensions;

public static class TimeIntervalListExtensions
{
    public static (TimeInterval? Sub, int index) BinarySearch(this List<TimeInterval> list, TimeSpan mediaTime)
    {
        int low = 0;
        int high = list.Count - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            var midVal = list[mid];

            if (midVal.ContainsTime(mediaTime))
            {
                return (midVal, mid);
            }
            else if (midVal.IsEarlierThan(mediaTime))
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

    public static (TimeInterval? Sub, int index) GetLaterClosest(this List<TimeInterval> list, TimeSpan mediaTime)
    {
        int low = 0;
        int high = list.Count - 1;
        int mid = low + (high - low) / 2;

        if (list.Count == 0)
            return (null, 0);

        if (list[0].StartTime >= mediaTime)
            return (list[0], 0);
        if (list[^1].EndTime <= mediaTime)
            return (null, -1);

        while (low < high)
        {
            var midVal = list[mid];

            if (midVal.IsEarlierThan(mediaTime))
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }

            mid = low + (high - low) / 2;
        }

        return (list[mid], mid);
    }

    public static void Insert(this List<TimeInterval> list, TimeInterval newInterval)
    {
        (_, int index) = list.GetLaterClosest(newInterval.EndTime);

        if (index == -1)
        {
            index = list.Count;
        }

        var overlapsOrAdjacentLeft = index > 0 && (list[index - 1].Overlaps(newInterval) ||
            list[index - 1].IsAdjacentTo(newInterval));

        var overlapsOrAdjacentRight = index < list.Count && (list[index].Overlaps(newInterval) ||
            list[index].IsAdjacentTo(newInterval));

        if (overlapsOrAdjacentLeft && overlapsOrAdjacentRight)
        {
            list[index - 1] = list[index - 1].Union(list[index]);
            list.RemoveAt(index);
        }
        else if (overlapsOrAdjacentLeft)
        {
            list[index - 1] = list[index - 1].Union(newInterval);
        }
        else if (overlapsOrAdjacentRight)
        {
            list[index] = list[index].Union(newInterval);
        }
        else
        {
            list.Insert(index, newInterval);
        }
    }
}
