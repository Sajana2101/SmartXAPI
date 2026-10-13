
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.Services;
using SmartX.Api.Services;

namespace SmartX.Api.Part2.Controllers
{
    [ApiController]
    [Route("api/device-health")]
    public class DeviceHealthController : ControllerBase
    {
        private readonly TelemetryDashboardService _dashboard;
        private readonly DeviceIssueTracker _issues;

        public DeviceHealthController(
            TelemetryDashboardService dashboard,
            DeviceIssueTracker issues)
        {
            _dashboard = dashboard;
            _issues = issues;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var dashboard = await _dashboard.GetDashboardAsync();

            return Ok(new
            {
                dashboard.GeneratedAtUtc,
                dashboard.TotalSensors,
                dashboard.ConnectedSensors,
                dashboard.AnomalyCount,
                activeIssues = _issues.ActiveCount,
                sensors = dashboard.Sensors
            });
        }

        [HttpPost("reconcile")]
        public async Task<IActionResult> Reconcile()
        {
            DeviceFaultReconciliationService service =
                new(_dashboard, _issues);

            return Ok(await service.ReconcileAsync());
        }
    }
}
