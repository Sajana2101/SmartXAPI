
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.DTOs;
using SmartX.Api.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;
using SmartX.Api.Part2.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryStore _store;
        private readonly ISensorRepository _sensors;
        private readonly TelemetryHistoryProcessor _history;
        private readonly TelemetrySeeder _seeder;
        private readonly TelemetryQueueService _queue;

        public TelemetryController(
            ITelemetryStore store,
            ISensorRepository sensors,
            TelemetryHistoryProcessor history,
            TelemetrySeeder seeder,
            TelemetryQueueService queue)
        {
            _store = store;
            _sensors = sensors;
            _history = history;
            _seeder = seeder;
            _queue = queue;
        }

        [HttpPost("seed/{readingsPerSensor:int}")]
        public async Task<IActionResult> SeedTelemetry(
            int readingsPerSensor,
            CancellationToken cancellationToken)
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

            try
            {
                string baseUrl =
                    $"{Request.Scheme}://{Request.Host}";

                int created = await _seeder.SeedAsync(
                    readingsPerSensor,
                    baseUrl,
                    cancellationToken);

                return Ok(new
                {
                    readingsCreated = created,
                    source = "HTTP API"
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    message =
                        "Simulated device requests could not reach the API."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("history/demo")]
        public IActionResult GetHistoryDemo()
        {
            float[][] batches =
            {
                new float[] { 21.5f, 21.8f, 22.0f },
                new float[] { 22.4f, 22.7f },
                new float[] { 23.1f, 23.5f, 23.7f, 24.0f }
            };

            List<float> readings =
                _history.ConvertBatchesToList(batches);

            return Ok(new
            {
                batchCount = batches.Length,
                readingCount = readings.Count,
                readings
            });
        }

        [HttpPost("float")]
        public async Task<IActionResult> AddFloat(
            FloatTelemetryRequest request)
        {
            if (await _sensors.GetByIdAsync(request.SensorId) is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Metric) ||
                !float.IsFinite(request.Value))
            {
                return BadRequest(new
                {
                    message = "A valid metric and finite value are required."
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

            _queue.EnqueueFloat(packet);
            _queue.ProcessPending();

            return Ok(packet);
        }

        [HttpPost("int")]
        public async Task<IActionResult> AddInt(
            IntTelemetryRequest request)
        {
            if (await _sensors.GetByIdAsync(request.SensorId) is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Metric))
            {
                return BadRequest(new
                {
                    message = "A valid metric is required."
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

            _queue.EnqueueInt(packet);
            _queue.ProcessPending();

            return Ok(packet);
        }

        [HttpPost("bool")]
        public async Task<IActionResult> AddBool(
            BoolTelemetryRequest request)
        {
            if (await _sensors.GetByIdAsync(request.SensorId) is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Metric))
            {
                return BadRequest(new
                {
                    message = "A valid metric is required."
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

            _queue.EnqueueBool(packet);
            _queue.ProcessPending();

            return Ok(packet);
        }

        [HttpGet("float")]
        public IActionResult GetFloatPackets()
        {
            return Ok(_store.GetFloatPackets());
        }

        [HttpGet("int")]
        public IActionResult GetIntPackets()
        {
            return Ok(_store.GetIntPackets());
        }

        [HttpGet("bool")]
        public IActionResult GetBoolPackets()
        {
            return Ok(_store.GetBoolPackets());
        }

        [HttpGet("power/calculate")]
        public IActionResult CalculatePower(
            int first,
            int second)
        {
            PowerReading firstReading = new(first);
            PowerReading secondReading = new(second);

            PowerReading combined = firstReading + secondReading;
            PowerReading delta = secondReading - firstReading;

            return Ok(new PowerCalculationResponse
            {
                FirstReading = firstReading.Watts,
                SecondReading = secondReading.Watts,
                CombinedPower = combined.Watts,
                Delta = delta.Watts
            });
        }
    }
}
