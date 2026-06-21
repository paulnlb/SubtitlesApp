using System.Collections.ObjectModel;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.Extensions;

public static class SubtitlesExtensions
{
    public static (T? Sub, int index) BinarySearch<T>(this ObservableCollection<T> list, TimeSpan mediaTime)
        where T : Subtitle
    {
        int low = 0;
        int high = list.Count - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            var midVal = list[mid];

            if (midVal.TimeInterval.ContainsTime(mediaTime))
            {
                return (midVal, mid);
            }
            else if (midVal.TimeInterval.IsEarlierThan(mediaTime))
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

    public static void Insert<T>(this ObservableCollection<T> list, T newSubtitle, bool removeOverlapping = true)
        where T : Subtitle
    {
        var insertionIndex = list.GetNextClosest(newSubtitle.TimeInterval.EndTime);

        if (insertionIndex == -1)
        {
            insertionIndex = list.Count;
        }

        if (!removeOverlapping)
        {
            list.Insert(insertionIndex, newSubtitle);
            return;
        }

        bool overlapsWithPrevious = false;
        bool overlapsWithNext = false;

        if (insertionIndex > 0)
        {
            overlapsWithPrevious = list[insertionIndex - 1].TimeInterval.Overlaps(newSubtitle.TimeInterval);
        }
        if (insertionIndex < list.Count)
        {
            overlapsWithNext = list[insertionIndex].TimeInterval.Overlaps(newSubtitle.TimeInterval);
        }

        if (overlapsWithPrevious)
        {
            list.RemoveAt(insertionIndex - 1);
            insertionIndex--;
        }
        if (overlapsWithNext)
        {
            list.RemoveAt(insertionIndex);
        }

        list.Insert(insertionIndex, newSubtitle);
    }

    public static void InsertMany<T>(this ObservableCollection<T> list, ObservableCollection<T> newItems)
        where T : Subtitle
    {
        foreach (var item in newItems)
        {
            list.Insert(item);
        }
    }

    public static int RemoveInside<T>(this ObservableCollection<T> list, TimeInterval timeInterval)
        where T : Subtitle
    {
        var itemsRemoved = 0;
        var startIndex = list.GetNextClosest(timeInterval.StartTime);

        if (startIndex == -1)
        {
            return itemsRemoved;
        }

        foreach (var item in list.Skip(startIndex).ToList())
        {
            if (timeInterval.IsEarlierThan(item.TimeInterval.StartTime))
            {
                return itemsRemoved;
            }

            list.Remove(item);
            itemsRemoved++;
        }

        return itemsRemoved;
    }

    private static int GetNextClosest<T>(this ObservableCollection<T> list, TimeSpan mediaTime)
        where T : Subtitle
    {
        int low = 0;
        int high = list.Count - 1;
        int mid = low + (high - low) / 2;

        if (list.Count == 0)
            return 0;

        if (list[0].TimeInterval.StartTime >= mediaTime)
            return 0;
        if (list[^1].TimeInterval.EndTime <= mediaTime)
            return -1;

        while (low < high)
        {
            var midVal = list[mid];

            if (midVal.TimeInterval.IsEarlierThan(mediaTime))
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }

            mid = low + (high - low) / 2;
        }

        return mid;
    }
}
