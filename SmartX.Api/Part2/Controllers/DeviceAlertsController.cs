
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.Models;
using SmartX.Api.Part2.Services;
using SmartX.Api.Repositories;

namespace SmartX.Api.Part2.Controllers
{
    [ApiController]
    [Route("api/device-alerts")]
    public class DeviceAlertsController : ControllerBase
    {
        private readonly PriorityAlertService _alerts;

        private readonly ISensorRepository _sensors;

        public DeviceAlertsController(
            PriorityAlertService alerts,
            ISensorRepository sensors)
        {
            _alerts = alerts;
            _sensors = sensors;
        }

        [HttpPost]
        public async Task<IActionResult> Enqueue(
            DeviceAlert alert)
        {
            if (alert.SensorId == Guid.Empty ||
                await _sensors.GetByIdAsync(alert.SensorId) is null)
            {
                return BadRequest(new
                {
                    message = "A valid sensor is required."
                });
            }

            if (!Enum.IsDefined(alert.Severity) ||
                string.IsNullOrWhiteSpace(alert.Message))
            {
                return BadRequest(new
                {
                    message = "Valid severity and message are required."
                });
            }

            alert.Id = Guid.NewGuid();
            alert.CreatedAtUtc = DateTime.UtcNow;
            alert.Message = alert.Message.Trim();

            _alerts.Enqueue(alert);

            return Accepted(new
            {
                alert.Id,
                alert.Severity,
                pendingCount = _alerts.PendingCount
            });
        }

        [HttpPost("process-next")]
        public IActionResult ProcessNext()
        {
            DeviceAlert? alert = _alerts.ProcessNext();

            return alert is null
                ? NoContent()
                : Ok(alert);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                pendingCount = _alerts.PendingCount,
                processed = _alerts.GetProcessed()
            });
        }
    }
}
