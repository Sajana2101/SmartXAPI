
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Part2.DTOs;
using SmartX.Api.Part2.Interfaces;
using SmartX.Api.Part2.Services;
using SmartX.Api.Repositories;

namespace SmartX.Api.Part2.Controllers
{
    [ApiController]
    [Route("api/device-commands")]
    public class DeviceCommandsController : ControllerBase
    {
        private readonly IDeviceCommandService _commands;

        private readonly ISensorRepository _sensors;

        public DeviceCommandsController(
            IDeviceCommandService commands,
            ISensorRepository sensors)
        {
            _commands = commands;
            _sensors = sensors;
        }

        [HttpPost]
        public async Task<IActionResult> Submit(
            CreateDeviceCommandRequest request)
        {
            try
            {
                var command = await _commands.SubmitAsync(request);

                return Ok(command);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
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

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            return Ok(await _commands.GetHistoryAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var command = await _commands.GetByIdAsync(id);

            return command is null
                ? NotFound()
                : Ok(command);
        }

        [HttpPost("undo")]
        public async Task<IActionResult> Undo()
        {
            var result = await _commands.UndoLastAsync();

            return result is null
                ? Conflict(new
                {
                    message = "No reversible command available."
                })
                : Ok(result);
        }

        [HttpGet("state/{sensorId:guid}")]
        public async Task<IActionResult> GetState(Guid sensorId)
        {
            if (await _sensors.GetByIdAsync(sensorId) is null)
            {
                return NotFound();
            }

            var service = (DeviceCommandService)_commands;

            return Ok(service.GetDeviceState(sensorId));
        }
    }
}
