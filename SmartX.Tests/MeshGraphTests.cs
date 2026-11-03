
using SmartX.Api.Part3.Topology;
using Xunit;

namespace SmartX.Tests;

public sealed class MeshGraphTests
{
    private static MeshGraph CreateGraph(params string[] sensorIds)
    {
        var graph = new MeshGraph();

        graph.AddNode(new MeshNode(
            "GATEWAY-01",
            "Main Gateway",
            "Core",
            true));

        foreach (var id in sensorIds)
        {
            graph.AddNode(new MeshNode(
                id,
                id,
                "Zone A"));
        }

        return graph;
    }

    [Fact]
    public void NewGraph_KeepsUnconnectedSensorsIsolated()
    {
        var graph = CreateGraph("A", "B");

        var traversal = graph.BreadthFirst("GATEWAY-01");

        Assert.Equal(3, graph.NodeCount);
        Assert.Equal(0, graph.LinkCount);
        Assert.Single(traversal.VisitOrder);
        Assert.Equal(2, traversal.UnreachableOnline.Count);
        Assert.Equal(3, graph.ConnectedComponents().Count);
    }

    [Fact]
    public void BreadthFirst_FindsMinimumHopCount()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink("GATEWAY-01", "A", 2);
        graph.AddLink("A", "C", 3);
        graph.AddLink("GATEWAY-01", "B", 1);
        graph.AddLink("B", "C", 2);

        var result = graph.BreadthFirst("GATEWAY-01");

        Assert.Equal(0, result.HopCounts["GATEWAY-01"]);
        Assert.Equal(1, result.HopCounts["A"]);
        Assert.Equal(1, result.HopCounts["B"]);
        Assert.Equal(2, result.HopCounts["C"]);
        Assert.Equal(4, result.VisitOrder.Count);
        Assert.Empty(result.UnreachableOnline);
    }

    [Fact]
    public void DepthFirst_VisitsOnlyReachableNodes()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink("GATEWAY-01", "A", 1);
        graph.AddLink("A", "B", 1);

        var result = graph.DepthFirst("GATEWAY-01");

        Assert.Equal(3, result.Count);
        Assert.Contains("GATEWAY-01", result);
        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.DoesNotContain("C", result);
    }

    [Fact]
    public void ConnectedComponents_DetectsSeparateNetworks()
    {
        var graph = CreateGraph("A", "B", "C", "D");

        graph.AddLink("GATEWAY-01", "A", 1);
        graph.AddLink("B", "C", 2);

        var components = graph.ConnectedComponents();

        Assert.Equal(3, components.Count);
        Assert.Contains(components, c =>
            c.Contains("GATEWAY-01") && c.Contains("A"));
        Assert.Contains(components, c =>
            c.Contains("B") && c.Contains("C"));
        Assert.Contains(components, c =>
            c.Count == 1 && c.Contains("D"));
    }

    [Fact]
    public void OfflineNode_BlocksTraversalThroughIt()
    {
        var graph = CreateGraph("A", "B");

        graph.AddLink("GATEWAY-01", "A", 1);
        graph.AddLink("A", "B", 1);

        graph.SetOnline("A", false);

        var result = graph.BreadthFirst("GATEWAY-01");

        Assert.Single(result.VisitOrder);
        Assert.Contains("A", result.Offline);
        Assert.Contains("B", result.UnreachableOnline);
        Assert.Equal(2, graph.ConnectedComponents().Count);
    }

    [Fact]
    public void RestoringNode_RecoversNetworkReachability()
    {
        var graph = CreateGraph("A", "B");

        graph.AddLink("GATEWAY-01", "A", 1);
        graph.AddLink("A", "B", 1);

        graph.SetOnline("A", false);

        Assert.Contains(
            "B",
            graph.BreadthFirst("GATEWAY-01").UnreachableOnline);

        graph.SetOnline("A", true);

        var recovered = graph.BreadthFirst("GATEWAY-01");

        Assert.Empty(recovered.UnreachableOnline);
        Assert.Equal(2, recovered.HopCounts["B"]);
    }

    [Fact]
    public void AddLink_RejectsDuplicateAndSelfConnections()
    {
        var graph = CreateGraph("A");

        graph.AddLink("GATEWAY-01", "A", 2);

        Assert.Throws<InvalidOperationException>(() =>
            graph.AddLink("A", "GATEWAY-01", 2));

        Assert.Throws<ArgumentException>(() =>
            graph.AddLink("A", "A", 1));

        Assert.Equal(1, graph.LinkCount);
    }

    [Fact]
    public void AddLink_RejectsInvalidCostsAndUnknownNodes()
    {
        var graph = CreateGraph("A");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            graph.AddLink("GATEWAY-01", "A", -1));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            graph.AddLink("GATEWAY-01", "A", double.NaN));

        Assert.Throws<KeyNotFoundException>(() =>
            graph.AddLink("GATEWAY-01", "UNKNOWN", 1));
    }

    [Fact]
    public void Graph_UsesCaseInsensitiveIdentifiers()
    {
        var graph = CreateGraph("AA:BB:CC:DD:EE:20");

        graph.AddLink(
            "gateway-01",
            "aa:bb:cc:dd:ee:20",
            2.5);

        Assert.True(graph.HasLink(
            "GATEWAY-01",
            "AA:BB:CC:DD:EE:20"));

        var result = graph.BreadthFirst("gateway-01");

        Assert.Equal(2, result.VisitOrder.Count);
        Assert.Empty(result.UnreachableOnline);
    }

    [Fact]
    public void LargeNetwork_TraversesWithoutRecursion()
    {
        var graph = CreateGraph(
            Enumerable.Range(1, 1000)
                .Select(i => $"NODE-{i}")
                .ToArray());

        graph.AddLink("GATEWAY-01", "NODE-1", 1);

        for (var i = 1; i < 1000; i++)
        {
            graph.AddLink(
                $"NODE-{i}",
                $"NODE-{i + 1}",
                1);
        }

        var bfs = graph.BreadthFirst("GATEWAY-01");
        var dfs = graph.DepthFirst("GATEWAY-01");

        Assert.Equal(1001, bfs.VisitOrder.Count);
        Assert.Equal(1000, bfs.HopCounts["NODE-1000"]);
        Assert.Equal(1001, dfs.Count);
        Assert.Single(graph.ConnectedComponents());
    }
}
