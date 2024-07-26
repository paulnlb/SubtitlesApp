namespace SubtitlesApp.Core.Models;

/// <summary>
///     TimeSet implementation using a LinkedList to store the intervals.
/// </summary>
public class TimeSetLinkedList : TimeSet
{
    private readonly LinkedList<TimeInterval> _timeIntervals = [];

    public override int Count { get => _timeIntervals.Count; }

    public override void Insert(TimeInterval newInterval)
    {
        (var leftNode, var rightNode) = GetNeighborNodesOf(newInterval);

        var nextNodeToLeft = leftNode == null ? _timeIntervals.First : leftNode.Next;

        if (nextNodeToLeft != null)
        {
            RemoveNodeIfOverlaps(nextNodeToLeft, ref newInterval);
        }

        var prevNodeToRight = rightNode == null ? _timeIntervals.Last : rightNode.Previous;

        if (prevNodeToRight != null)
        {
            RemoveNodeIfOverlaps(prevNodeToRight, ref newInterval);
        }

        RemoveSubListBetween(leftNode, rightNode);

        if (leftNode?.List == null)
        {
            _timeIntervals.AddFirst(newInterval);
        }
        else
        {
            _timeIntervals.AddAfter(leftNode, newInterval);
        }
    }

    public override (TimeInterval? Interval, int index) GetByTimeStamp(TimeSpan timeStamp)
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

    (LinkedListNode<TimeInterval>? LeftNode, LinkedListNode<TimeInterval>? RightNode) GetNeighborNodesOf(TimeInterval newInterval)
    {
        var currentNode = _timeIntervals.First;
        LinkedListNode<TimeInterval>? leftNode = null;
        LinkedListNode<TimeInterval>? rightNode = null;

        while (currentNode != null)
        {
            if (currentNode.Value.IsEarlierThan(newInterval.StartTime))
            {
                leftNode = currentNode;
            }
            else if (currentNode.Value.IsLaterThan(newInterval.EndTime))
            {
                rightNode = currentNode;
                break;
            }

            currentNode = currentNode.Next;
        }

        return (leftNode, rightNode);
    }

    void RemoveSubListBetween(LinkedListNode<TimeInterval>? start, LinkedListNode<TimeInterval>? end)
    {
        var currentNode = start == null ? _timeIntervals.First : start.Next;

        while (currentNode != end && currentNode != null)
        {
            var nextNode = currentNode.Next;
            _timeIntervals.Remove(currentNode);
            currentNode = nextNode;
        }
    }

    /// <summary>
    ///     Removes the node if it overlaps with the interval and updates the interval to be the union of the two.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="interval"></param>
    void RemoveNodeIfOverlaps(LinkedListNode<TimeInterval> node, ref TimeInterval interval)
    {
        if (node.Value.OverlapsOrAdjacentTo(interval))
        {
            interval = interval.Union(node.Value);
            _timeIntervals.Remove(node);
        }
    }
}
