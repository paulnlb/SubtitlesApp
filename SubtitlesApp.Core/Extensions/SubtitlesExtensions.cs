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

    public static void Insert<T>(this ObservableCollection<T> list, T newSubtitle)
        where T : Subtitle
    {
        bool overlapsWithPrevious = false;
        bool overlapsWithNext = false;

        var insertionIndex = list.GetNextClosest(newSubtitle.TimeInterval.EndTime);

        if (insertionIndex == -1)
        {
            insertionIndex = list.Count;
        }

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
