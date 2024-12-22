using SubtitlesApp.Core.Models;
using System.Collections.ObjectModel;

namespace SubtitlesApp.Core.Extensions;

public static class SubtitlesExtensions
{
    public static (T? Sub, int index) BinarySearch<T>(this ObservableCollection<T> list, TimeSpan mediaTime) where T : Subtitle
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

    public static (T? Sub, int index) GetLaterClosest<T>(this ObservableCollection<T> list, TimeSpan mediaTime) where T : Subtitle
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

    public static void Insert<T>(this ObservableCollection<T> list, T newSubtitle) where T: Subtitle
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

    public static void SwitchToTranslations<T>(this ObservableCollection<T> list, int skip = 0) where T : Subtitle
    {
        while (skip < list.Count)
        {
            list[skip].SwitchToTranslation();
            skip++;
        }
    }

    public static void RestoreOriginalLanguages<T>(this ObservableCollection<T> list, int skip = 0) where T : Subtitle
    {
        while (skip < list.Count)
        {
            list[skip].RestoreOriginalLanguage();
            skip++;
        }
    }

    public static void InsertMany<T>(this ObservableCollection<T> list, ObservableCollection<T> newItems) where T : Subtitle
    {
        foreach (var item in newItems)
        {
            list.Insert(item);
        }
    }

    public static void ReplaceMany<T>(this ObservableCollection<T> list, ObservableCollection<T> newItems, int skip = 0) where T : Subtitle
    {
        if (newItems.Count != list.Count - skip)
        {
            throw new ArgumentException("ReplaceMany failed: new items lists sizes do not match");
        }

        for (int i = 0; i < newItems.Count; i++)
        {
            list[skip + i] = newItems[i];
        }
    }
}
