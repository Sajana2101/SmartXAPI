
namespace SmartX.Api.Part3.Topology;

public sealed record MeshNode(
    string Id,
    string Label,
    string Zone,
    bool IsGateway = false,
    bool IsOnline = true);

public sealed record MeshLink(
    string From,
    string To,
    double Cost,
    int RssiDbm = -65);

public sealed record MeshBfsResult(
    string Source,
    IReadOnlyList<string> VisitOrder,
    IReadOnlyDictionary<string, int> HopCounts,
    IReadOnlyDictionary<string, string?> Parents,
    IReadOnlyList<string> UnreachableOnline,
    IReadOnlyList<string> Offline);

public sealed class MeshGraph
{
    private readonly Dictionary<string, MeshNode> _nodes =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, List<MeshLink>> _adjacency =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly List<MeshLink> _links = [];

    public IReadOnlyList<MeshNode> Nodes =>
        _nodes.Values
            .OrderBy(n => n.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public IReadOnlyList<MeshLink> Links => _links.ToArray();

    public int NodeCount => _nodes.Count;

    public int LinkCount => _links.Count;

    public bool ContainsNode(string id)
    {
        return !string.IsNullOrWhiteSpace(id) &&
               _nodes.ContainsKey(id);
    }

    public bool TryGetNode(string id, out MeshNode? node)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            node = null;
            return false;
        }

        return _nodes.TryGetValue(id, out node);
    }

    public void AddNode(MeshNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (string.IsNullOrWhiteSpace(node.Id))
        {
            throw new ArgumentException(
                "A node must have an identifier.");
        }

        if (!_nodes.TryAdd(node.Id, node))
        {
            throw new InvalidOperationException(
                "A node with this identifier already exists.");
        }

        _adjacency.Add(node.Id, []);
    }

    public void SetOnline(string id, bool isOnline)
    {
        if (!_nodes.TryGetValue(id, out var node))
        {
            throw new KeyNotFoundException(
                "The requested node does not exist.");
        }

        _nodes[id] = node with { IsOnline = isOnline };
    }

    public bool HasLink(string from, string to)
    {
        if (!_adjacency.TryGetValue(from, out var links))
        {
            return false;
        }

        return links.Any(link =>
            string.Equals(
                Other(link, from),
                to,
                StringComparison.OrdinalIgnoreCase));
    }

    public void AddLink(
        string from,
        string to,
        double cost,
        int rssiDbm = -65)
    {
        if (!double.IsFinite(cost) || cost <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cost),
                "Link cost must be a positive finite number.");
        }

        if (!ContainsNode(from) || !ContainsNode(to))
        {
            throw new KeyNotFoundException(
                "Both nodes must exist before creating a link.");
        }

        if (string.Equals(
            from, to, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "A node cannot connect to itself.");
        }

        if (HasLink(from, to))
        {
            throw new InvalidOperationException(
                "This wireless link already exists.");
        }

        var link = new MeshLink(from, to, cost, rssiDbm);

        _links.Add(link);
        _adjacency[from].Add(link);
        _adjacency[to].Add(link);
    }

    public MeshBfsResult BreadthFirst(string source)
    {
        if (!_nodes.TryGetValue(source, out var start))
        {
            throw new KeyNotFoundException(
                "The starting node does not exist.");
        }

        var visited = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var hops = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        var parents = new Dictionary<string, string?>(
            StringComparer.OrdinalIgnoreCase);

        var order = new List<string>();
        var queue = new Queue<string>();

        if (start.IsOnline)
        {
            visited.Add(start.Id);
            hops[start.Id] = 0;
            parents[start.Id] = null;
            queue.Enqueue(start.Id);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            order.Add(current);

            foreach (var neighbour in OnlineNeighbours(current))
            {
                if (!visited.Add(neighbour))
                {
                    continue;
                }

                hops[neighbour] = hops[current] + 1;
                parents[neighbour] = current;
                queue.Enqueue(neighbour);
            }
        }

        var unreachable = _nodes.Values
            .Where(n => n.IsOnline && !visited.Contains(n.Id))
            .Select(n => n.Id)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var offline = _nodes.Values
            .Where(n => !n.IsOnline)
            .Select(n => n.Id)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new MeshBfsResult(
            start.Id,
            order,
            hops,
            parents,
            unreachable,
            offline);
    }

    public IReadOnlyList<string> DepthFirst(string source)
    {
        if (!_nodes.TryGetValue(source, out var start))
        {
            throw new KeyNotFoundException(
                "The starting node does not exist.");
        }

        var order = new List<string>();

        if (!start.IsOnline)
        {
            return order;
        }

        var visited = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var stack = new Stack<string>();
        stack.Push(start.Id);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (!visited.Add(current))
            {
                continue;
            }

            order.Add(current);

            foreach (var neighbour in
                OnlineNeighbours(current).Reverse())
            {
                if (!visited.Contains(neighbour))
                {
                    stack.Push(neighbour);
                }
            }
        }

        return order;
    }

    public IReadOnlyList<IReadOnlyList<string>> ConnectedComponents()
    {
        var components = new List<IReadOnlyList<string>>();

        var visited = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var node in Nodes.Where(n => n.IsOnline))
        {
            if (visited.Contains(node.Id))
            {
                continue;
            }

            var component = DepthFirst(node.Id);

            foreach (var id in component)
            {
                visited.Add(id);
            }

            components.Add(component);
        }

        return components;
    }

    private IEnumerable<string> OnlineNeighbours(string id)
    {
        return _adjacency[id]
            .Select(link => Other(link, id))
            .Where(next => _nodes[next].IsOnline)
            .OrderBy(next => next, StringComparer.OrdinalIgnoreCase);
    }

    private static string Other(MeshLink link, string id)
    {
        return string.Equals(
            link.From, id, StringComparison.OrdinalIgnoreCase)
            ? link.To
            : link.From;
    }
}
