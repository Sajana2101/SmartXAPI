using Microsoft.AspNetCore.Mvc;
using SmartX.Api.DTOs;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/telemetry-dashboard")]
    public class TelemetryDashboardController :
        ControllerBase
    {
        private readonly
            TelemetryDashboardService
            _dashboardService;

        public TelemetryDashboardController(
            TelemetryDashboardService
                dashboardService)
        {
            _dashboardService =
                dashboardService;
        }

        [HttpGet]
        public async Task<
            ActionResult<
                TelemetryDashboardResponse>>
            GetDashboard()
        {
            TelemetryDashboardResponse
                dashboard =
                    await _dashboardService
                        .GetDashboardAsync();

            return Ok(dashboard);
        }
    }
}