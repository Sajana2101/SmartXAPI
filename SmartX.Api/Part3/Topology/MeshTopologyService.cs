
using SmartX.Api.Repositories;

namespace SmartX.Api.Part3.Topology;

public sealed record MeshOverview(
    string GatewayId,
    IReadOnlyList<MeshNode> Nodes,
    IReadOnlyList<MeshLink> Links,
    MeshBfsResult GatewayReachability,
    IReadOnlyList<IReadOnlyList<string>> ConnectedComponents);

public sealed class MeshTopologyService
{
    public const string GatewayId = "GATEWAY-01";

    private readonly ISensorRepository _sensors;
    private readonly object _sync = new();

    private readonly Dictionary<string, MeshLink> _links =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _offline =
        new(StringComparer.OrdinalIgnoreCase);

    public MeshTopologyService(ISensorRepository sensors)
    {
        _sensors = sensors;
    }

    public async Task<MeshGraph> BuildGraphAsync()
    {
        var sensors = await _sensors.GetAllAsync();

        MeshLink[] links;
        HashSet<string> offline;

        lock (_sync)
        {
            links = _links.Values.ToArray();

            offline = new HashSet<string>(
                _offline,
                StringComparer.OrdinalIgnoreCase);
        }

        var graph = new MeshGraph();

        graph.AddNode(new MeshNode(
            GatewayId,
            "Main Gateway",
            "Core",
            true,
            true));

        foreach (var sensor in sensors)
        {
            var id = sensor.DeviceIdentifier.Trim();

            var label = string.IsNullOrWhiteSpace(sensor.NodeId)
                ? id
                : sensor.NodeId;

            graph.AddNode(new MeshNode(
                id,
                label,
                sensor.Zone,
                false,
                !offline.Contains(id)));
        }

        foreach (var link in links)
        {
            if (graph.ContainsNode(link.From) &&
                graph.ContainsNode(link.To))
            {
                graph.AddLink(
                    link.From,
                    link.To,
                    link.Cost,
                    link.RssiDbm);
            }
        }

        return graph;
    }

    public async Task<MeshOverview> GetOverviewAsync()
    {
        var graph = await BuildGraphAsync();

        return new MeshOverview(
            GatewayId,
            graph.Nodes,
            graph.Links,
            graph.BreadthFirst(GatewayId),
            graph.ConnectedComponents());
    }

    public bool TryAddLink(
        string from,
        string to,
        double cost,
        int rssiDbm)
    {
        var key = LinkKey(from, to);

        var link = new MeshLink(
            from.Trim(),
            to.Trim(),
            cost,
            rssiDbm);

        lock (_sync)
        {
            return _links.TryAdd(key, link);
        }
    }

    public bool RemoveLink(string from, string to)
    {
        lock (_sync)
        {
            return _links.Remove(LinkKey(from, to));
        }
    }

    public void SetSensorOnline(string id, bool isOnline)
    {
        lock (_sync)
        {
            if (isOnline)
            {
                _offline.Remove(id);
            }
            else
            {
                _offline.Add(id);
            }
        }
    }

    private static string LinkKey(string from, string to)
    {
        var first = from.Trim().ToUpperInvariant();
        var second = to.Trim().ToUpperInvariant();

        return string.CompareOrdinal(first, second) <= 0
            ? $"{first}|{second}"
            : $"{second}|{first}";
    }
}
