
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.Services;

namespace SmartX.Api.Part2.Controllers
{
    [ApiController]
    [Route("api/telemetry-queue")]
    public class TelemetryQueueController : ControllerBase
    {
        private readonly TelemetryQueueService _queue;

        public TelemetryQueueController(
            TelemetryQueueService queue)
        {
            _queue = queue;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                pendingCount = _queue.PendingCount,
                processedCount = _queue.ProcessedCount
            });
        }

        [HttpPost("process")]
        public IActionResult ProcessPending()
        {
            int processed = _queue.ProcessPending();

            return Ok(new
            {
                processed,
                remaining = _queue.PendingCount
            });
        }
    }
}
