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
        private readonly TelemetryHistoryProcessor _historyProcessor;
        private readonly TelemetrySeeder _telemetrySeeder;

        public TelemetryController(
            ITelemetryStore telemetryStore,
            ISensorRepository sensorRepository,
            TelemetryHistoryProcessor historyProcessor,
            TelemetrySeeder telemetrySeeder)
        {
            _telemetryStore = telemetryStore;
            _sensorRepository = sensorRepository;
            _historyProcessor = historyProcessor;
            _telemetrySeeder = telemetrySeeder;
        }
        [HttpPost("seed/{readingsPerSensor:int}")]
        public async Task<IActionResult> SeedTelemetry(
    int readingsPerSensor)
        {
            if (readingsPerSensor < 1 ||
                readingsPerSensor > 10000)
            {
                return BadRequest(new
                {
                    message =
                        "Readings per sensor must be between 1 and 10000."
                });
            }

            int created =
                await _telemetrySeeder.SeedAsync(readingsPerSensor);

            return Ok(new
            {
                readingsCreated = created
            });
        }
        [HttpGet("history/demo")]
        public ActionResult<object> GetHistoryDemo()
        {
            float[][] historicalBatches =
            {
        new float[] { 21.5f, 21.8f, 22.0f },
        new float[] { 22.4f, 22.7f },
        new float[] { 23.1f, 23.5f, 23.7f, 24.0f }
    };

            List<float> processedReadings =
                _historyProcessor.ConvertBatchesToList(historicalBatches);

            return Ok(new
            {
                batchCount = historicalBatches.Length,
                readingCount = processedReadings.Count,
                readings = processedReadings
            });
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


        [HttpGet("power/calculate")]
        public ActionResult<PowerCalculationResponse> CalculatePower(
    int first,
    int second)
        {
            PowerReading firstReading = new(first);
            PowerReading secondReading = new(second);

            PowerReading combined = firstReading + secondReading;
            PowerReading delta = secondReading - firstReading;

            PowerCalculationResponse response = new()
            {
                FirstReading = firstReading.Watts,
                SecondReading = secondReading.Watts,
                CombinedPower = combined.Watts,
                Delta = delta.Watts
            };

            return Ok(response);
        }
    }
}