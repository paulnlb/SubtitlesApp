using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using System.Collections.ObjectModel;

namespace SubtitlesApp.Shared.Extensions;

public static class SubtitlesExtensions
{
    public static (Subtitle? Sub, int index) BinarySearch(this ObservableCollection<Subtitle> list, TimeSpan mediaTime)
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

    public static (Subtitle? Sub, int index) GetLaterClosest(this ObservableCollection<Subtitle> list, TimeSpan mediaTime)
    {
        int low = 0;
        int high = list.Count - 1;
        int mid = low + (high - low) / 2;

        if (list.Count == 0)
            return (null, 0);

        if (list[0].TimeInterval.StartTime >= mediaTime)
            return (list[0], 0);
        if (list[^1].TimeInterval.EndTime <= mediaTime)
            return (null, -1);

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

        return (list[mid], mid);
    }

    public static void Insert(this ObservableCollection<Subtitle> list, Subtitle newSubtitle)
    {
        bool overlapsWithLeft = false;
        bool overlapsWithRight = false;

        (_, int index) = list.GetLaterClosest(newSubtitle.TimeInterval.EndTime);

        if (index == -1)
        {
            index = list.Count;
        }

        if (index > 0)
        {
            overlapsWithLeft = list[index - 1].TimeInterval.Overlaps(newSubtitle.TimeInterval);
        }
        if (index < list.Count)
        {
            overlapsWithRight = list[index].TimeInterval.Overlaps(newSubtitle.TimeInterval);
        }

        if (overlapsWithLeft)
        {
            list.RemoveAt(index - 1);
            index--;
        }
        if (overlapsWithRight)
        {
            list.RemoveAt(index);
        }

        list.Insert(index, newSubtitle);
    }
}
