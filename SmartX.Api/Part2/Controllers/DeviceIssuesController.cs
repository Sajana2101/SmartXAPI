
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.Models;
using SmartX.Api.Part2.Services;
using SmartX.Api.Repositories;

namespace SmartX.Api.Part2.Controllers
{
    [ApiController]
    [Route("api/device-issues")]
    public class DeviceIssuesController : ControllerBase
    {
        private readonly DeviceIssueTracker _tracker;

        private readonly ISensorRepository _sensors;

        public DeviceIssuesController(
            DeviceIssueTracker tracker,
            ISensorRepository sensors)
        {
            _tracker = tracker;
            _sensors = sensors;
        }

        [HttpGet]
        public IActionResult GetActive()
        {
            return Ok(new
            {
                activeCount = _tracker.ActiveCount,
                issues = _tracker.GetActive()
            });
        }

        [HttpPost("{sensorId:guid}/{type}")]
        public async Task<IActionResult> Report(
            Guid sensorId,
            DeviceIssueType type)
        {
            if (!Enum.IsDefined(type))
            {
                return BadRequest(new
                {
                    message = "Invalid issue type."
                });
            }

            if (await _sensors.GetByIdAsync(sensorId) is null)
            {
                return NotFound(new
                {
                    message = "Sensor not found."
                });
            }

            bool added = _tracker.Report(sensorId, type);

            return Ok(new
            {
                added,
                activeCount = _tracker.ActiveCount
            });
        }

        [HttpDelete("{sensorId:guid}/{type}")]
        public IActionResult Resolve(
            Guid sensorId,
            DeviceIssueType type)
        {
            if (!Enum.IsDefined(type))
            {
                return BadRequest(new
                {
                    message = "Invalid issue type."
                });
            }

            bool resolved = _tracker.Resolve(sensorId, type);

            return Ok(new
            {
                resolved,
                activeCount = _tracker.ActiveCount
            });
        }
    }
}
