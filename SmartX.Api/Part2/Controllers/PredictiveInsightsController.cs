
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.Models;
using SmartX.Api.Part2.Services;
using SmartX.Api.Repositories;

namespace SmartX.Api.Part2.Controllers
{
    [ApiController]
    [Route("api/predictive-insights")]
    public class PredictiveInsightsController : ControllerBase
    {
        private readonly PredictiveRecommendationService _engine;
        private readonly DeviceSearchHistoryService _searches;
        private readonly ISensorRepository _sensors;

        public PredictiveInsightsController(
            PredictiveRecommendationService engine,
            DeviceSearchHistoryService searches,
            ISensorRepository sensors)
        {
            _engine = engine;
            _searches = searches;
            _sensors = sensors;
        }

        [HttpGet]
        public async Task<IActionResult> GetInsights()
        {
            return Ok(await _engine.GenerateAsync());
        }

        [HttpGet("search-history")]
        public IActionResult GetSearchHistory()
        {
            return Ok(_searches.GetHistory());
        }

        [HttpPost("search")]
        public async Task<IActionResult> RecordSearch(
            [FromBody] RecordDeviceSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query) ||
                request.Query.Trim().Length > 150)
            {
                return BadRequest(new
                {
                    message =
                        "Search query must contain 1 to 150 characters."
                });
            }

            if (request.SensorId.HasValue &&
                await _sensors.GetByIdAsync(
                    request.SensorId.Value) is null)
            {
                return NotFound(new
                {
                    message = "The selected sensor does not exist."
                });
            }

            DeviceSearchEvent result = _searches.Record(
                request.Query,
                request.SensorId);

            return Ok(result);
        }
    }
}
