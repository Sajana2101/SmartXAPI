using Microsoft.AspNetCore.Mvc;
using SmartX.Api.DTOs;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController : ControllerBase
    {
        private readonly ISensorRepository _sensorRepository;

        public SensorsController(ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Sensor>>> GetSensors()
        {
            IReadOnlyList<Sensor> sensors =
                await _sensorRepository.GetAllAsync();

            return Ok(sensors);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Sensor>> GetSensor(Guid id)
        {
            Sensor? sensor = await _sensorRepository.GetByIdAsync(id);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            return Ok(sensor);
        }

        [HttpPost]
        public async Task<ActionResult<Sensor>> CreateSensor(
            CreateSensorRequest request)
        {
            string deviceIdentifier = request.DeviceIdentifier.Trim();

            Sensor? existingSensor =
                await _sensorRepository.GetByDeviceIdentifierAsync(
                    deviceIdentifier);

            if (existingSensor is not null)
            {
                return Conflict(new
                {
                    message =
                        "A sensor with this device identifier already exists."
                });
            }

            DateTime now = DateTime.UtcNow;

            Sensor sensor = new()
            {
                Id = Guid.NewGuid(),
                DeviceIdentifier = deviceIdentifier,
                DeploymentLocation = request.DeploymentLocation.Trim(),
                Room = request.Room.Trim(),
                Zone = request.Zone.Trim(),
                NodeId = request.NodeId.Trim(),
                Category = request.Category,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            await _sensorRepository.AddAsync(sensor);

            return CreatedAtAction(
                nameof(GetSensor),
                new { id = sensor.Id },
                sensor);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Sensor>> UpdateSensor(
            Guid id,
            UpdateSensorRequest request)
        {
            Sensor? existingSensor =
                await _sensorRepository.GetByIdAsync(id);

            if (existingSensor is null)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            string deviceIdentifier = request.DeviceIdentifier.Trim();

            Sensor? duplicate =
                await _sensorRepository.GetByDeviceIdentifierAsync(
                    deviceIdentifier);

            if (duplicate is not null && duplicate.Id != id)
            {
                return Conflict(new
                {
                    message =
                        "A sensor with this device identifier already exists."
                });
            }

            existingSensor.DeviceIdentifier = deviceIdentifier;
            existingSensor.DeploymentLocation =
                request.DeploymentLocation.Trim();
            existingSensor.Room = request.Room.Trim();
            existingSensor.Zone = request.Zone.Trim();
            existingSensor.NodeId = request.NodeId.Trim();
            existingSensor.Category = request.Category;
            existingSensor.UpdatedAtUtc = DateTime.UtcNow;

            await _sensorRepository.UpdateAsync(existingSensor);

            return Ok(existingSensor);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSensor(Guid id)
        {
            bool deleted = await _sensorRepository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Sensor could not be found."
                });
            }

            return NoContent();
        }
    }
}