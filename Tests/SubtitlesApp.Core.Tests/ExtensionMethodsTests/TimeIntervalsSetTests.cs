using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.Tests.ExtensionMethodsTests;

public class TimeIntervalsSetTests
{
    TimeSet _timeSet;

    [SetUp]
    public void Setup()
    {
        _timeSet = new TimeSet();
    }

    [Test(Description = "Insert one time interval in empty TimeSet")]
    public void InsertOneInterval()
    {
        var timeInterval = new TimeInterval(0, 10);

        _timeSet.Insert(timeInterval);
        (var intervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(5));

        Assert.Multiple(() =>
        {
            Assert.That(_timeSet.Count, Is.EqualTo(1));
            Assert.That(intervalFromList, Is.EqualTo(timeInterval));
        });
    }

    [Test(Description = "Insert two NON overlapping time intervals in TimeSet")]
    public void InsertTwoNonOverlappingIntervals_ShoulResultTwoIntervals()
    {
        var timeInterval1 = new TimeInterval(0, 10);
        var timeInterval2 = new TimeInterval(11, 20);

        _timeSet.Insert(timeInterval1);
        _timeSet.Insert(timeInterval2);

        (var intervalFromList1, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(5));
        (var intervalFromList2, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(15));

        Assert.Multiple(() =>
        {
            Assert.That(_timeSet.Count, Is.EqualTo(2));
            Assert.That(intervalFromList1, Is.EqualTo(timeInterval1));
            Assert.That(intervalFromList2, Is.EqualTo(timeInterval2));
        });
    }

    [Test(Description = "Insert two overlapping time intervals in TimeSet")]
    public void InsertTwoOverlappingIntervals_ShouldResultOneInterval()
    {
        var timeInterval1 = new TimeInterval(0, 10);
        var timeInterval2 = new TimeInterval(5, 15);

        _timeSet.Insert(timeInterval1);
        _timeSet.Insert(timeInterval2);

        (var intervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(10));

        Assert.That(_timeSet.Count, Is.EqualTo(1));
        Assert.That(intervalFromList, Is.Not.Null);
        Assert.That(intervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(intervalFromList.ContainsTime(TimeSpan.FromSeconds(14)));
    }

    [Test(Description = "Insert two adjacent time intervals in TimeSet")]
    public void InsertTwoAdjacentIntervals_ShouldResultOneInterval()
    {
        var timeInterval1 = new TimeInterval(0, 10);
        var timeInterval2 = new TimeInterval(10, 20);

        _timeSet.Insert(timeInterval1);
        _timeSet.Insert(timeInterval2);

        (var intervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(10));

        Assert.That(_timeSet.Count, Is.EqualTo(1));
        Assert.That(intervalFromList, Is.Not.Null);
        Assert.That(intervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(intervalFromList.ContainsTime(TimeSpan.FromSeconds(19)));
    }

    [Test(Description = "Insert two NON overlapping intervals " +
        "and a third interval in TimeSet, which overlaps left and right")]
    public void InsertThreeIntervalsWithOverlappingLeftAndRight_ShouldResultOneInterval()
    {
        var leftInterval = new TimeInterval(0, 10);
        var middleInterval = new TimeInterval(5, 15);
        var rightInterval = new TimeInterval(12, 20);

        _timeSet.Insert(leftInterval);
        _timeSet.Insert(rightInterval);

        _timeSet.Insert(middleInterval);

        (var intervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(12));

        Assert.That(_timeSet.Count, Is.EqualTo(1));
        Assert.That(intervalFromList, Is.Not.Null);
        Assert.That(intervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(intervalFromList.ContainsTime(TimeSpan.FromSeconds(19)));
    }

    [Test(Description = "Insert two NON overlapping intervals " +
        "and a third interval in TimeSet, which overlaps left")]
    public void InsertThreeIntervalsWithOverlappingLeft_ShouldResultTwoIntervals()
    {
        var leftInterval = new TimeInterval(0, 10);
        var middleInterval = new TimeInterval(9, 13);
        var rightInterval = new TimeInterval(15, 20);

        _timeSet.Insert(leftInterval);
        _timeSet.Insert(rightInterval);

        _timeSet.Insert(middleInterval);

        (var leftIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(5));
        (var rightIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(16));

        Assert.That(_timeSet.Count, Is.EqualTo(2));
        Assert.That(leftIntervalFromList, Is.Not.Null);
        Assert.That(rightIntervalFromList, Is.Not.Null);

        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.FromSeconds(12)));

        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(15)));
        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(19)));
    }

    [Test(Description = "Insert two NON overlapping intervals " +
        "and a third interval in TimeSet, which overlaps right")]
    public void InsertThreeIntervalsWithOverlappingRight_ShouldResultTwoIntervals()
    {
        var leftInterval = new TimeInterval(0, 10);
        var middleInterval = new TimeInterval(12, 16);
        var rightInterval = new TimeInterval(15, 20);

        _timeSet.Insert(leftInterval);
        _timeSet.Insert(rightInterval);

        _timeSet.Insert(middleInterval);

        (var leftIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(5));
        (var rightIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(16));

        Assert.That(_timeSet.Count, Is.EqualTo(2));
        Assert.That(leftIntervalFromList, Is.Not.Null);
        Assert.That(rightIntervalFromList, Is.Not.Null);

        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.FromSeconds(9)));

        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(12)));
        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(19)));
    }

    [Test(Description = "Insert two NON overlapping intervals " +
        "and a third interval in TimeSet, which is adjacent to the left")]
    public void InsertThreeIntervalsWithAdjacentLeft_ShouldResultTwoIntervals()
    {
        var leftInterval = new TimeInterval(0, 10);
        var middleInterval = new TimeInterval(10, 15);
        var rightInterval = new TimeInterval(20, 30);

        _timeSet.Insert(leftInterval);
        _timeSet.Insert(rightInterval);

        _timeSet.Insert(middleInterval);

        (var leftIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(5));
        (var rightIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(25));

        Assert.That(_timeSet.Count, Is.EqualTo(2));
        Assert.That(leftIntervalFromList, Is.Not.Null);
        Assert.That(rightIntervalFromList, Is.Not.Null);

        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.FromSeconds(14)));

        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(20)));
        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(29)));
    }

    [Test(Description = "Insert two NON overlapping intervals " +
        "and a third interval in TimeSet, which is adjacent to the right")]
    public void InsertThreeIntervalsWithAdjacentRight_ShouldResultTwoIntervals()
    {
        var leftInterval = new TimeInterval(0, 10);
        var middleInterval = new TimeInterval(15, 20);
        var rightInterval = new TimeInterval(20, 30);

        _timeSet.Insert(leftInterval);
        _timeSet.Insert(rightInterval);

        _timeSet.Insert(middleInterval);

        (var leftIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(5));
        (var rightIntervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(25));

        Assert.That(_timeSet.Count, Is.EqualTo(2));
        Assert.That(leftIntervalFromList, Is.Not.Null);
        Assert.That(rightIntervalFromList, Is.Not.Null);

        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(leftIntervalFromList.ContainsTime(TimeSpan.FromSeconds(9)));

        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(15)));
        Assert.That(rightIntervalFromList.ContainsTime(TimeSpan.FromSeconds(29)));
    }

    [Test(Description = "Insert two NON overlapping intervals " +
        "and a third interval in TimeSet, which is adjacent to the left and right")]
    public void InsertThreeIntervalsWithAdjacentLeftAndRight_ShouldResultOneInterval()
    {
        var leftInterval = new TimeInterval(0, 10);
        var middleInterval = new TimeInterval(10, 15);
        var rightInterval = new TimeInterval(15, 20);

        _timeSet.Insert(leftInterval);
        _timeSet.Insert(rightInterval);

        _timeSet.Insert(middleInterval);

        (var intervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(12));

        Assert.That(_timeSet.Count, Is.EqualTo(1));
        Assert.That(intervalFromList, Is.Not.Null);
        Assert.That(intervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(intervalFromList.ContainsTime(TimeSpan.FromSeconds(19)));
    }

    [Test(Description = "Insert two NON overlapping intervals" +
        "and a third interval in TimeSet, which overlaps and contains both left and right")]
    public void InsertThreeIntervalsWithOverlappingAndContainingLeftAndRight_ShouldResultOneInterval()
    {
        var leftInterval = new TimeInterval(5, 10);
        var bigInterval = new TimeInterval(0, 25);
        var rightInterval = new TimeInterval(15, 20);

        _timeSet.Insert(leftInterval);
        _timeSet.Insert(rightInterval);

        _timeSet.Insert(bigInterval);

        (var intervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(12));

        Assert.That(_timeSet.Count, Is.EqualTo(1));
        Assert.That(intervalFromList, Is.Not.Null);
        Assert.That(intervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(intervalFromList.ContainsTime(TimeSpan.FromSeconds(24)));
    }

    [Test(Description = "Insert four NON overlapping intervals" +
        "and a fifth interval in TimeSet, which overlaps and contains all of them")]
    public void InsertFiveIntervalsWithOverlappingAndContainingAll_ShouldResultOneInterval()
    {
        var interval1 = new TimeInterval(0, 5);
        var interval2 = new TimeInterval(6, 10);
        var interval3 = new TimeInterval(11, 15);
        var interval4 = new TimeInterval(16, 20);
        var bigInterval = new TimeInterval(0, 20);

        _timeSet.Insert(interval1);
        _timeSet.Insert(interval2);
        _timeSet.Insert(interval3);
        _timeSet.Insert(interval4);

        _timeSet.Insert(bigInterval);

        (var intervalFromList, _) = _timeSet.GetByTimeStamp(TimeSpan.FromSeconds(12));

        Assert.That(_timeSet.Count, Is.EqualTo(1));
        Assert.That(intervalFromList, Is.Not.Null);
        Assert.That(intervalFromList.ContainsTime(TimeSpan.Zero));
        Assert.That(intervalFromList.ContainsTime(TimeSpan.FromSeconds(19)));
    }
}