using System.Collections.Generic;

namespace SubtitlesApp.Core.Models;

/// <summary>
///     Represents a set of time intervals. The intervals are stored in LinkedList and are sorted by their start time.
/// </summary>
public class TimeSet
{
    private readonly LinkedList<TimeInterval> _timeIntervals = [];

    public int Count { get => _timeIntervals.Count; }

    public (TimeInterval? Interval, int index) GetByTimeStamp(TimeSpan timeStamp)
    {
        var currentNode = _timeIntervals.First;
        var index = 0;

        while (currentNode != null)
        {
            if (currentNode.Value.ContainsTime(timeStamp))
            {
                return (currentNode.Value, index);
            }

            currentNode = currentNode.Next;
            index++;
        }

        return (null, -1);
    }

    public void Insert(TimeInterval newInterval)
    {
        (var insertAfterNode, var updatedInterval) = UpdateAndFindInsertionPoint(newInterval);

        if (insertAfterNode?.List == null)
        {
            _timeIntervals.AddFirst(updatedInterval);
        }
        else
        {
            _timeIntervals.AddAfter(insertAfterNode, updatedInterval);
        }
    }

    (LinkedListNode<TimeInterval>? InsertAfterNode, TimeInterval UpdatedInterval) UpdateAndFindInsertionPoint(TimeInterval intervalToInsert)
    {
        var currentNode = _timeIntervals.First;
        LinkedListNode<TimeInterval>? insertAfterNode = null;

        while (currentNode != null)
        {
            var currentInterval = currentNode.Value;

            if (currentInterval.OverlapsOrAdjacentTo(intervalToInsert))
            {
                intervalToInsert = intervalToInsert.Union(currentInterval);

                currentNode = RemoveCurrentNode(currentNode);

                continue;
            }
            else if (currentInterval.IsEarlierThan(intervalToInsert.StartTime))
            {
                insertAfterNode = currentNode;
            }
            else if (currentInterval.IsLaterThan(intervalToInsert.EndTime))
            {
                break;
            }

            currentNode = currentNode.Next;
        }

        return (insertAfterNode, intervalToInsert);
    }

    /// <summary>
    /// Removes the current node from the linked list and returns the next node.
    /// </summary>
    /// <param name="currentNode">The current node to be removed.</param>
    /// <returns>The next node in the linked list.</returns>
    private LinkedListNode<TimeInterval>? RemoveCurrentNode(LinkedListNode<TimeInterval> currentNode)
    {
        var nextNode = currentNode.Next;
        _timeIntervals.Remove(currentNode);
        return nextNode;
    }
}
