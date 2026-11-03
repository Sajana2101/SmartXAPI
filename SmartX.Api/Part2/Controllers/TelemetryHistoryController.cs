
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.Services;

namespace SmartX.Api.Part2.Controllers;

[ApiController]
[Route("api/telemetry-history")]
public class TelemetryHistoryController : ControllerBase
{
    private readonly SortedTelemetryHistoryService _history;

    public TelemetryHistoryController(
        SortedTelemetryHistoryService history)
    {
        _history = history;
    }

    [HttpGet]
    public IActionResult GetHistory(
        [FromQuery] Guid? sensorId)
    {
        return Ok(_history.GetHistory(sensorId));
    }

    [HttpGet("count")]
    public IActionResult GetCount()
    {
        return Ok(new
        {
            count = _history.Count
        });
    }

    [HttpGet("range")]
    public IActionResult GetRange(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] Guid? sensorId,
        [FromQuery] int limit = 500)
    {
        if (!fromUtc.HasValue || !toUtc.HasValue)
        {
            return BadRequest(new
            {
                message = "Both fromUtc and toUtc are required."
            });
        }

        if (limit < 1 || limit > 10000)
        {
            return BadRequest(new
            {
                message = "Limit must be between 1 and 10000."
            });
        }

        if (fromUtc.Value.ToUniversalTime() >
            toUtc.Value.ToUniversalTime())
        {
            return BadRequest(new
            {
                message = "The start date must not exceed the end date."
            });
        }

        return Ok(_history.GetRange(
            fromUtc.Value,
            toUtc.Value,
            sensorId,
            limit));
    }

    [HttpGet("avl-stats")]
    public IActionResult GetAvlStatistics()
    {
        return Ok(_history.GetStatistics());
    }
}
