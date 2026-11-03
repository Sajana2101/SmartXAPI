
namespace SmartX.Api.Part3.Topology;

public sealed record MeshMstResult(
    string GatewayId,
    IReadOnlyList<MeshLink> SelectedLinks,
    double TotalCost,
    IReadOnlyList<string> ConnectedNodes,
    IReadOnlyList<string> UnreachableOnline,
    IReadOnlyList<string> OfflineNodes,
    int OnlineComponentCount,
    bool SpansAllOnlineNodes);

public sealed record MeshRouteResult(
    string Source,
    string Destination,
    bool Reachable,
    IReadOnlyList<string> Path,
    IReadOnlyList<MeshLink> Links,
    double? TotalCost,
    int HopCount,
    string Message);

public static class MeshRoutingAlgorithms
{
    private static readonly StringComparer IdComparer =
        StringComparer.OrdinalIgnoreCase;

    public static MeshMstResult PrimMinimumSpanningTree(
        MeshGraph graph,
        string gatewayId)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (!graph.TryGetNode(gatewayId, out var gateway) ||
            gateway is null)
        {
            throw new KeyNotFoundException(
                "The gateway does not exist.");
        }

        var nodes = graph.Nodes;

        var online = new HashSet<string>(
            nodes.Where(node => node.IsOnline)
                .Select(node => node.Id),
            IdComparer);

        var adjacency = BuildAdjacency(graph, online);

        var connected = new HashSet<string>(IdComparer);
        var selected = new List<MeshLink>();
        var pending = new PriorityQueue<MeshLink, double>();

        double totalCost = 0;

        void EnqueueLinks(string nodeId)
        {
            foreach (var link in adjacency[nodeId])
            {
                var next = OtherEndpoint(link, nodeId);

                if (!connected.Contains(next))
                {
                    pending.Enqueue(link, link.Cost);
                }
            }
        }

        if (gateway.IsOnline)
        {
            connected.Add(gateway.Id);
            EnqueueLinks(gateway.Id);
        }

        while (pending.Count > 0)
        {
            var link = pending.Dequeue();

            var fromConnected = connected.Contains(link.From);
            var toConnected = connected.Contains(link.To);

            if (fromConnected == toConnected)
            {
                continue;
            }

            var next = fromConnected ? link.To : link.From;

            if (!online.Contains(next))
            {
                continue;
            }

            connected.Add(next);
            selected.Add(link);
            totalCost += link.Cost;

            EnqueueLinks(next);
        }

        var unreachable = online
            .Where(id => !connected.Contains(id))
            .OrderBy(id => id, IdComparer)
            .ToArray();

        var offline = nodes
            .Where(node => !node.IsOnline)
            .Select(node => node.Id)
            .OrderBy(id => id, IdComparer)
            .ToArray();

        return new MeshMstResult(
            gateway.Id,
            selected,
            totalCost,
            connected.OrderBy(id => id, IdComparer).ToArray(),
            unreachable,
            offline,
            graph.ConnectedComponents().Count,
            unreachable.Length == 0 && gateway.IsOnline);
    }

    public static MeshRouteResult DijkstraShortestPath(
        MeshGraph graph,
        string source,
        string destination)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (!graph.TryGetNode(source, out var start) ||
            start is null)
        {
            throw new KeyNotFoundException(
                "The source node does not exist.");
        }

        if (!graph.TryGetNode(destination, out var target) ||
            target is null)
        {
            throw new KeyNotFoundException(
                "The destination node does not exist.");
        }

        MeshRouteResult Unreachable(string message)
        {
            return new MeshRouteResult(
                start.Id,
                target.Id,
                false,
                Array.Empty<string>(),
                Array.Empty<MeshLink>(),
                null,
                0,
                message);
        }

        if (!start.IsOnline)
        {
            return Unreachable("The source node is offline.");
        }

        if (!target.IsOnline)
        {
            return Unreachable("The destination node is offline.");
        }

        if (IdComparer.Equals(start.Id, target.Id))
        {
            return new MeshRouteResult(
                start.Id,
                target.Id,
                true,
                new[] { start.Id },
                Array.Empty<MeshLink>(),
                0,
                0,
                "The source is already at the destination.");
        }

        var online = new HashSet<string>(
            graph.Nodes
                .Where(node => node.IsOnline)
                .Select(node => node.Id),
            IdComparer);

        var adjacency = BuildAdjacency(graph, online);

        var distances = new Dictionary<string, double>(IdComparer);
        var previous = new Dictionary<
            string, (string Parent, MeshLink Link)>(IdComparer);

        var pending = new PriorityQueue<string, double>();

        foreach (var nodeId in online)
        {
            distances[nodeId] = double.PositiveInfinity;
        }

        distances[start.Id] = 0;
        pending.Enqueue(start.Id, 0);

        while (pending.TryDequeue(out var current, out var cost))
        {
            if (cost > distances[current])
            {
                continue;
            }

            if (IdComparer.Equals(current, target.Id))
            {
                break;
            }

            foreach (var link in adjacency[current])
            {
                var neighbour = OtherEndpoint(link, current);
                var candidate = cost + link.Cost;

                if (candidate >= distances[neighbour])
                {
                    continue;
                }

                distances[neighbour] = candidate;
                previous[neighbour] = (current, link);
                pending.Enqueue(neighbour, candidate);
            }
        }

        if (double.IsPositiveInfinity(distances[target.Id]))
        {
            return Unreachable(
                "No available route connects these nodes.");
        }

        var path = new List<string>();
        var routeLinks = new List<MeshLink>();

        var cursor = target.Id;
        path.Add(cursor);

        while (!IdComparer.Equals(cursor, start.Id))
        {
            if (!previous.TryGetValue(cursor, out var step))
            {
                return Unreachable(
                    "A complete route could not be reconstructed.");
            }

            routeLinks.Add(step.Link);
            cursor = step.Parent;
            path.Add(cursor);
        }

        path.Reverse();
        routeLinks.Reverse();

        return new MeshRouteResult(
            start.Id,
            target.Id,
            true,
            path,
            routeLinks,
            distances[target.Id],
            routeLinks.Count,
            "A minimum-cost route is available.");
    }

    private static Dictionary<string, List<MeshLink>> BuildAdjacency(
        MeshGraph graph,
        HashSet<string> online)
    {
        var adjacency =
            new Dictionary<string, List<MeshLink>>(IdComparer);

        foreach (var id in online)
        {
            adjacency[id] = new List<MeshLink>();
        }

        foreach (var link in graph.Links)
        {
            if (!online.Contains(link.From) ||
                !online.Contains(link.To))
            {
                continue;
            }

            if (!double.IsFinite(link.Cost) || link.Cost <= 0)
            {
                continue;
            }

            adjacency[link.From].Add(link);
            adjacency[link.To].Add(link);
        }

        foreach (var links in adjacency.Values)
        {
            links.Sort((left, right) =>
            {
                var comparison = left.Cost.CompareTo(right.Cost);

                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = IdComparer.Compare(left.From, right.From);

                return comparison != 0
                    ? comparison
                    : IdComparer.Compare(left.To, right.To);
            });
        }

        return adjacency;
    }

    private static string OtherEndpoint(
        MeshLink link,
        string nodeId)
    {
        return IdComparer.Equals(link.From, nodeId)
            ? link.To
            : link.From;
    }
}
