
using SmartX.Api.Part3.Topology;
using Xunit;

namespace SmartX.Tests;

public sealed class MeshRoutingTests
{
    private const string Gateway = "GATEWAY-01";

    private static MeshGraph CreateGraph(params string[] sensorIds)
    {
        var graph = new MeshGraph();

        graph.AddNode(new MeshNode(
            Gateway,
            "Main Gateway",
            "Core",
            true));

        foreach (var id in sensorIds)
        {
            graph.AddNode(new MeshNode(id, id, "Zone A"));
        }

        return graph;
    }

    [Fact]
    public void Prim_SelectsMinimumCostConnections()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink(Gateway, "A", 8);
        graph.AddLink(Gateway, "B", 2);
        graph.AddLink("A", "B", 1);
        graph.AddLink("B", "C", 3);
        graph.AddLink("A", "C", 10);

        var result =
            MeshRoutingAlgorithms.PrimMinimumSpanningTree(
                graph, Gateway);

        Assert.True(result.SpansAllOnlineNodes);
        Assert.Equal(3, result.SelectedLinks.Count);
        Assert.Equal(6d, result.TotalCost);
        Assert.Equal(4, result.ConnectedNodes.Count);
        Assert.Empty(result.UnreachableOnline);
    }

    [Fact]
    public void Prim_DetectsDisconnectedSensors()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink(Gateway, "A", 2);
        graph.AddLink("A", "B", 1);

        var result =
            MeshRoutingAlgorithms.PrimMinimumSpanningTree(
                graph, Gateway);

        Assert.False(result.SpansAllOnlineNodes);
        Assert.Contains("C", result.UnreachableOnline);
        Assert.Equal(2, result.SelectedLinks.Count);
        Assert.Equal(3d, result.TotalCost);
        Assert.Equal(2, result.OnlineComponentCount);
    }

    [Fact]
    public void Prim_ExcludesOfflineNodes()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink(Gateway, "A", 1);
        graph.AddLink("A", "C", 1);
        graph.AddLink(Gateway, "B", 2);
        graph.AddLink("B", "C", 3);

        graph.SetOnline("A", false);

        var result =
            MeshRoutingAlgorithms.PrimMinimumSpanningTree(
                graph, Gateway);

        Assert.Contains("A", result.OfflineNodes);
        Assert.DoesNotContain("A", result.ConnectedNodes);
        Assert.Equal(5d, result.TotalCost);
        Assert.Equal(2, result.SelectedLinks.Count);
        Assert.True(result.SpansAllOnlineNodes);
    }

    [Fact]
    public void Prim_HandlesOfflineGateway()
    {
        var graph = CreateGraph("A", "B");

        graph.AddLink(Gateway, "A", 1);
        graph.AddLink("A", "B", 2);

        graph.SetOnline(Gateway, false);

        var result =
            MeshRoutingAlgorithms.PrimMinimumSpanningTree(
                graph, Gateway);

        Assert.False(result.SpansAllOnlineNodes);
        Assert.Empty(result.SelectedLinks);
        Assert.Equal(0d, result.TotalCost);
        Assert.Contains(Gateway, result.OfflineNodes);
        Assert.Equal(2, result.UnreachableOnline.Count);
    }

    [Fact]
    public void Dijkstra_SelectsLowestCostInsteadOfFewestHops()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink(Gateway, "C", 9);
        graph.AddLink(Gateway, "B", 2);
        graph.AddLink("B", "C", 3);
        graph.AddLink(Gateway, "A", 1);
        graph.AddLink("A", "C", 10);

        var result =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "C", Gateway);

        Assert.True(result.Reachable);
        Assert.Equal(5d, result.TotalCost);
        Assert.Equal(2, result.HopCount);
        Assert.Equal(
            new[] { "C", "B", Gateway },
            result.Path);
    }

    [Fact]
    public void Dijkstra_ReroutesWhenIntermediateNodeFails()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink(Gateway, "A", 1);
        graph.AddLink("A", "C", 1);
        graph.AddLink(Gateway, "B", 2);
        graph.AddLink("B", "C", 3);

        var original =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "C", Gateway);

        Assert.Equal(2d, original.TotalCost);
        Assert.Equal(
            new[] { "C", "A", Gateway },
            original.Path);

        graph.SetOnline("A", false);

        var rerouted =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "C", Gateway);

        Assert.True(rerouted.Reachable);
        Assert.Equal(5d, rerouted.TotalCost);
        Assert.Equal(
            new[] { "C", "B", Gateway },
            rerouted.Path);
    }

    [Fact]
    public void Dijkstra_ReportsUnreachableWhenBridgeFails()
    {
        var graph = CreateGraph("A", "B");

        graph.AddLink(Gateway, "A", 1);
        graph.AddLink("A", "B", 1);

        graph.SetOnline("A", false);

        var result =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "B", Gateway);

        Assert.False(result.Reachable);
        Assert.Null(result.TotalCost);
        Assert.Empty(result.Path);
        Assert.Empty(result.Links);
    }

    [Fact]
    public void Dijkstra_RecoversOriginalRouteAfterRestoration()
    {
        var graph = CreateGraph("A", "B", "C");

        graph.AddLink(Gateway, "A", 1);
        graph.AddLink("A", "C", 1);
        graph.AddLink(Gateway, "B", 2);
        graph.AddLink("B", "C", 3);

        graph.SetOnline("A", false);

        var failed =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "C", Gateway);

        Assert.Equal(5d, failed.TotalCost);

        graph.SetOnline("A", true);

        var recovered =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "C", Gateway);

        Assert.True(recovered.Reachable);
        Assert.Equal(2d, recovered.TotalCost);
        Assert.Equal(
            new[] { "C", "A", Gateway },
            recovered.Path);
    }

    [Fact]
    public void Dijkstra_SourceEqualsDestination()
    {
        var graph = CreateGraph("A");

        var result =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "A", "A");

        Assert.True(result.Reachable);
        Assert.Equal(0d, result.TotalCost);
        Assert.Equal(0, result.HopCount);
        Assert.Equal(new[] { "A" }, result.Path);
    }

    [Fact]
    public void Dijkstra_RejectsUnknownNodes()
    {
        var graph = CreateGraph("A");

        Assert.Throws<KeyNotFoundException>(() =>
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "UNKNOWN", Gateway));

        Assert.Throws<KeyNotFoundException>(() =>
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "A", "UNKNOWN"));
    }

    [Fact]
    public void Dijkstra_RejectsOfflineSource()
    {
        var graph = CreateGraph("A");

        graph.AddLink(Gateway, "A", 1);
        graph.SetOnline("A", false);

        var result =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph, "A", Gateway);

        Assert.False(result.Reachable);
        Assert.Null(result.TotalCost);
        Assert.Contains("offline", result.Message);
    }

    [Fact]
    public void Routing_SupportsCaseInsensitiveMacAddresses()
    {
        var graph = CreateGraph("AA:BB:CC:DD:EE:20");

        graph.AddLink(
            Gateway,
            "AA:BB:CC:DD:EE:20",
            2.5);

        var result =
            MeshRoutingAlgorithms.DijkstraShortestPath(
                graph,
                "aa:bb:cc:dd:ee:20",
                "gateway-01");

        Assert.True(result.Reachable);
        Assert.Equal(2.5d, result.TotalCost);
        Assert.Equal(1, result.HopCount);
    }
}
