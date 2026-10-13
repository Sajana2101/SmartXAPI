
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.Services;

namespace SmartX.Api.Part2.Controllers
{
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
    }
}
