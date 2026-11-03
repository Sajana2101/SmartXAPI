
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part3.Topology;

namespace SmartX.Api.Part3.Controllers;

public sealed record MeshRoutingOverview(
    MeshMstResult MinimumSpanningTree,
    IReadOnlyList<MeshRouteResult> SensorRoutes);

[ApiController]
[Route("api/mesh/routing")]
public sealed class MeshRoutingController : ControllerBase
{
    private readonly MeshTopologyService _topology;

    public MeshRoutingController(MeshTopologyService topology)
    {
        _topology = topology;
    }

    [HttpGet("mst")]
    public async Task<IActionResult> GetMinimumSpanningTree()
    {
        var graph = await _topology.BuildGraphAsync();

        var result = MeshRoutingAlgorithms.PrimMinimumSpanningTree(
            graph,
            MeshTopologyService.GatewayId);

        return Ok(result);
    }

    [HttpGet("route")]
    public async Task<IActionResult> GetShortestRoute(
        [FromQuery] string? source,
        [FromQuery] string? destination)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return BadRequest(new
            {
                message = "A source node is required."
            });
        }

        var target = string.IsNullOrWhiteSpace(destination)
            ? MeshTopologyService.GatewayId
            : destination.Trim();

        var graph = await _topology.BuildGraphAsync();

        if (!graph.ContainsNode(source.Trim()))
        {
            return NotFound(new
            {
                message = "The source node does not exist."
            });
        }

        if (!graph.ContainsNode(target))
        {
            return NotFound(new
            {
                message = "The destination node does not exist."
            });
        }

        var route = MeshRoutingAlgorithms.DijkstraShortestPath(
            graph,
            source.Trim(),
            target);

        return Ok(route);
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetRoutingOverview()
    {
        var graph = await _topology.BuildGraphAsync();

        var gatewayId = MeshTopologyService.GatewayId;

        var mst = MeshRoutingAlgorithms.PrimMinimumSpanningTree(
            graph,
            gatewayId);

        var routes = graph.Nodes
            .Where(node => !node.IsGateway)
            .Select(node =>
                MeshRoutingAlgorithms.DijkstraShortestPath(
                    graph,
                    node.Id,
                    gatewayId))
            .ToArray();

        return Ok(new MeshRoutingOverview(mst, routes));
    }
}
