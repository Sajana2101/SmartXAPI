
using SmartX.Api.Part2.Services;
using SmartX.Api.Part3.Structures;
using Xunit;

namespace SmartX.Tests;

public sealed class AvlTreeTests
{
    private static readonly DateTime BaseTime =
        new(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(30, 20, 10)]
    [InlineData(10, 20, 30)]
    [InlineData(30, 10, 20)]
    [InlineData(10, 30, 20)]
    public void Insert_RotatesTreeAndMaintainsBalance(
        int first,
        int second,
        int third)
    {
        var tree = new AvlTree<int>();

        tree.Insert(BaseTime.AddSeconds(first), first);
        tree.Insert(BaseTime.AddSeconds(second), second);
        tree.Insert(BaseTime.AddSeconds(third), third);

        Assert.Equal(3, tree.Count);
        Assert.Equal(2, tree.Height);
        Assert.True(tree.Rotations > 0);
        Assert.True(tree.IsBalanced());
    }

    [Fact]
    public void Insert_AllowsDuplicateTimestamps()
    {
        var tree = new AvlTree<string>();

        tree.Insert(BaseTime, "first");
        tree.Insert(BaseTime, "second");
        tree.Insert(BaseTime, "third");

        var results = tree.GetRange(BaseTime, BaseTime);

        Assert.Equal(3, tree.Count);
        Assert.Equal(1, tree.Height);
        Assert.True(tree.IsBalanced());

        Assert.Equal(
            new[] { "third", "second", "first" },
            results);
    }

    [Fact]
    public void GetRange_ReturnsOnlyMatchingTimestamps()
    {
        var tree = new AvlTree<int>();

        for (var i = 0; i < 100; i++)
        {
            tree.Insert(BaseTime.AddMinutes(i), i);
        }

        var results = tree.GetRange(
            BaseTime.AddMinutes(20),
            BaseTime.AddMinutes(30),
            newestFirst: false);

        Assert.Equal(
            Enumerable.Range(20, 11),
            results);

        Assert.True(tree.IsBalanced());
    }

    [Fact]
    public void GetRange_SupportsNewestFirstAndLimit()
    {
        var tree = new AvlTree<int>();

        for (var i = 0; i < 20; i++)
        {
            tree.Insert(BaseTime.AddSeconds(i), i);
        }

        var results = tree.GetRange(
            BaseTime,
            BaseTime.AddSeconds(19),
            limit: 5);

        Assert.Equal(
            new[] { 19, 18, 17, 16, 15 },
            results);
    }

    [Fact]
    public void LargeHistory_RemainsBalanced()
    {
        var tree = new AvlTree<int>();

        for (var i = 0; i < 1000; i++)
        {
            tree.Insert(BaseTime.AddSeconds(i), i);
        }

        Assert.Equal(1000, tree.Count);
        Assert.True(tree.IsBalanced());
        Assert.InRange(tree.Height, 1, 20);
        Assert.True(tree.Rotations > 0);

        var results = tree.GetRange(
            BaseTime.AddSeconds(200),
            BaseTime.AddSeconds(209),
            newestFirst: false);

        Assert.Equal(
            Enumerable.Range(200, 10),
            results);
    }

    [Fact]
    public void GetRange_RejectsInvalidDateRange()
    {
        var tree = new AvlTree<int>();

        Assert.Throws<ArgumentException>(() =>
            tree.GetRange(
                BaseTime.AddHours(1),
                BaseTime));
    }

    [Fact]
    public void EmptyTree_IsBalancedAndReturnsNoReadings()
    {
        var tree = new AvlTree<int>();

        var results = tree.GetRange(
            BaseTime,
            BaseTime.AddHours(1));

        Assert.Empty(results);
        Assert.Equal(0, tree.Count);
        Assert.Equal(0, tree.Height);
        Assert.True(tree.IsBalanced());
    }

    [Fact]
    public void HistoryService_FiltersBySensorAndTimestamp()
    {
        var service = new SortedTelemetryHistoryService();

        var firstSensor = Guid.NewGuid();
        var secondSensor = Guid.NewGuid();

        service.Record(
            firstSensor,
            "temperature",
            24.5f,
            BaseTime);

        service.Record(
            secondSensor,
            "humidity",
            54.0f,
            BaseTime.AddMinutes(1));

        service.Record(
            firstSensor,
            "temperature",
            26.0f,
            BaseTime.AddMinutes(2));

        var results = service.GetRange(
            BaseTime,
            BaseTime.AddMinutes(2),
            firstSensor);

        Assert.Equal(2, results.Count);

        Assert.All(results, reading =>
            Assert.Equal(firstSensor, reading.SensorId));

        Assert.Equal(
            new[] { "26", "24.5" },
            results.Select(reading => reading.Value));

        var statistics = service.GetStatistics();

        Assert.Equal(3, statistics.TotalReadings);
        Assert.Equal(2, statistics.IndexedSensors);
        Assert.True(statistics.Balanced);
    }
}
