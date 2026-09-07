using Microsoft.AspNetCore.Mvc;
using SmartX.Api.DTOs;
using SmartX.Api.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryStore _telemetryStore;
        private readonly ISensorRepository _sensorRepository;

        public TelemetryController(
            ITelemetryStore telemetryStore,
            ISensorRepository sensorRepository)
        {
            _telemetryStore = telemetryStore;
            _sensorRepository = sensorRepository;
        }

        [HttpPost("float")]
        public async Task<ActionResult<TelemetryPacket<float>>> AddFloat(
            FloatTelemetryRequest request)
        {
            Sensor? sensor =
                await _sensorRepository.GetByIdAsync(request.SensorId);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            TelemetryPacket<float> packet = new()
            {
                Id = Guid.NewGuid(),
                SensorId = request.SensorId,
                Metric = request.Metric.Trim(),
                Value = request.Value,
                TimestampUtc = DateTime.UtcNow
            };

            _telemetryStore.AddFloat(packet);

            return Ok(packet);
        }

        [HttpPost("int")]
        public async Task<ActionResult<TelemetryPacket<int>>> AddInt(
            IntTelemetryRequest request)
        {
            Sensor? sensor =
                await _sensorRepository.GetByIdAsync(request.SensorId);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            TelemetryPacket<int> packet = new()
            {
                Id = Guid.NewGuid(),
                SensorId = request.SensorId,
                Metric = request.Metric.Trim(),
                Value = request.Value,
                TimestampUtc = DateTime.UtcNow
            };

            _telemetryStore.AddInt(packet);

            return Ok(packet);
        }

        [HttpPost("bool")]
        public async Task<ActionResult<TelemetryPacket<bool>>> AddBool(
            BoolTelemetryRequest request)
        {
            Sensor? sensor =
                await _sensorRepository.GetByIdAsync(request.SensorId);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            TelemetryPacket<bool> packet = new()
            {
                Id = Guid.NewGuid(),
                SensorId = request.SensorId,
                Metric = request.Metric.Trim(),
                Value = request.Value,
                TimestampUtc = DateTime.UtcNow
            };

            _telemetryStore.AddBool(packet);

            return Ok(packet);
        }

        [HttpGet("float")]
        public ActionResult<
            IReadOnlyList<TelemetryPacket<float>>> GetFloatPackets()
        {
            return Ok(_telemetryStore.GetFloatPackets());
        }

        [HttpGet("int")]
        public ActionResult<
            IReadOnlyList<TelemetryPacket<int>>> GetIntPackets()
        {
            return Ok(_telemetryStore.GetIntPackets());
        }

        [HttpGet("bool")]
        public ActionResult<
            IReadOnlyList<TelemetryPacket<bool>>> GetBoolPackets()
        {
            return Ok(_telemetryStore.GetBoolPackets());
        }
    }
}