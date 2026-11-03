
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part3.Topology;

namespace SmartX.Api.Part3.Controllers;

public sealed record MeshLinkRequest(
    string From,
    string To,
    double Cost,
    int RssiDbm = -65);

public sealed record MeshNodeStatusRequest(
    string Id,
    bool IsOnline);

[ApiController]
[Route("api/mesh")]
public sealed class MeshController : ControllerBase
{
    private readonly MeshTopologyService _topology;

    public MeshController(MeshTopologyService topology)
    {
        _topology = topology;
    }

    [HttpGet]
    public async Task<IActionResult> GetTopology()
    {
        return Ok(await _topology.GetOverviewAsync());
    }

    [HttpGet("bfs")]
    public async Task<IActionResult> GetBfs(
        [FromQuery] string? source)
    {
        var graph = await _topology.BuildGraphAsync();

        var start = string.IsNullOrWhiteSpace(source)
            ? MeshTopologyService.GatewayId
            : source.Trim();

        if (!graph.ContainsNode(start))
        {
            return NotFound(new
            {
                message = "The starting node does not exist."
            });
        }

        return Ok(graph.BreadthFirst(start));
    }

    [HttpGet("dfs")]
    public async Task<IActionResult> GetDfs(
        [FromQuery] string? source)
    {
        var graph = await _topology.BuildGraphAsync();

        var start = string.IsNullOrWhiteSpace(source)
            ? MeshTopologyService.GatewayId
            : source.Trim();

        if (!graph.ContainsNode(start))
        {
            return NotFound(new
            {
                message = "The starting node does not exist."
            });
        }

        return Ok(new
        {
            source = start,
            visitOrder = graph.DepthFirst(start),
            connectedComponents = graph.ConnectedComponents()
        });
    }

    [HttpPost("links")]
    public async Task<IActionResult> CreateLink(
        [FromBody] MeshLinkRequest request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.From) ||
            string.IsNullOrWhiteSpace(request.To))
        {
            return BadRequest(new
            {
                message = "Both node identifiers are required."
            });
        }

        if (!double.IsFinite(request.Cost) ||
            request.Cost <= 0)
        {
            return BadRequest(new
            {
                message = "Link cost must be greater than zero."
            });
        }

        var from = request.From.Trim();
        var to = request.To.Trim();

        if (string.Equals(
            from, to, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "A node cannot link to itself."
            });
        }

        var graph = await _topology.BuildGraphAsync();

        if (!graph.ContainsNode(from) ||
            !graph.ContainsNode(to))
        {
            return NotFound(new
            {
                message = "One or both nodes do not exist."
            });
        }

        if (graph.HasLink(from, to))
        {
            return Conflict(new
            {
                message = "This wireless link already exists."
            });
        }

        var created = _topology.TryAddLink(
            from,
            to,
            request.Cost,
            request.RssiDbm);

        if (!created)
        {
            return Conflict(new
            {
                message = "This wireless link already exists."
            });
        }

        return Created(
            "/api/mesh",
            new MeshLink(
                from,
                to,
                request.Cost,
                request.RssiDbm));
    }

    [HttpDelete("links")]
    public async Task<IActionResult> DeleteLink(
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (string.IsNullOrWhiteSpace(from) ||
            string.IsNullOrWhiteSpace(to))
        {
            return BadRequest(new
            {
                message = "Both node identifiers are required."
            });
        }

        var graph = await _topology.BuildGraphAsync();

        if (!graph.HasLink(from, to))
        {
            return NotFound(new
            {
                message = "The wireless link does not exist."
            });
        }

        _topology.RemoveLink(from, to);

        return NoContent();
    }

    [HttpPut("nodes/status")]
    public async Task<IActionResult> SetNodeStatus(
        [FromBody] MeshNodeStatusRequest request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Id))
        {
            return BadRequest(new
            {
                message = "A node identifier is required."
            });
        }

        var graph = await _topology.BuildGraphAsync();

        if (!graph.TryGetNode(request.Id, out var node) ||
            node is null)
        {
            return NotFound(new
            {
                message = "The node does not exist."
            });
        }

        if (node.IsGateway)
        {
            return BadRequest(new
            {
                message = "Gateway status cannot be changed here."
            });
        }

        _topology.SetSensorOnline(
            node.Id,
            request.IsOnline);

        return Ok(await _topology.GetOverviewAsync());
    }
}
