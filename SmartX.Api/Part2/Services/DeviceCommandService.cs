
using SmartX.Api.Part2.DTOs;
using SmartX.Api.Part2.Interfaces;
using SmartX.Api.Part2.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Part2.Services
{
    public class DeviceCommandService : IDeviceCommandService
    {
        private sealed class DeviceState
        {
            public bool Enabled { get; set; } = true;

            public int SamplingIntervalSeconds { get; set; } = 30;
        }

        private sealed record UndoEntry(
            Guid CommandId,
            Guid SensorId,
            bool PreviousEnabled,
            int PreviousInterval);

        private readonly ISensorRepository _sensors;

        private readonly Dictionary<Guid, DeviceState> _states = new();

        private readonly List<DeviceCommand> _history = new();

        private readonly Stack<UndoEntry> _undoStack = new();

        private readonly object _lock = new();

        public DeviceCommandService(
            ISensorRepository sensors)
        {
            _sensors = sensors;
        }

        public async Task<DeviceCommand> SubmitAsync(
            CreateDeviceCommandRequest request)
        {
            if (!request.SensorId.HasValue ||
                request.SensorId.Value == Guid.Empty ||
                !request.Type.HasValue ||
                !Enum.IsDefined(request.Type.Value))
            {
                throw new ArgumentException(
                    "Valid sensor ID and command type are required.");
            }

            Guid sensorId = request.SensorId.Value;

            if (await _sensors.GetByIdAsync(sensorId) is null)
            {
                throw new KeyNotFoundException(
                    "The selected sensor does not exist.");
            }

            DeviceCommandType type = request.Type.Value;

            if (type == DeviceCommandType.SetSamplingInterval &&
                (!request.SamplingIntervalSeconds.HasValue ||
                 request.SamplingIntervalSeconds.Value < 1 ||
                 request.SamplingIntervalSeconds.Value > 86400))
            {
                throw new ArgumentException(
                    "Sampling interval must be between 1 and 86400 seconds.");
            }

            lock (_lock)
            {
                if (!_states.TryGetValue(
                    sensorId, out DeviceState? state))
                {
                    state = new DeviceState();
                    _states.Add(sensorId, state);
                }

                DeviceCommand command = new()
                {
                    SensorId = sensorId,
                    Type = type,
                    Status = DeviceCommandStatus.Queued,
                    SamplingIntervalSeconds =
                        request.SamplingIntervalSeconds
                };

                _history.Add(command);

                command.Status = DeviceCommandStatus.Executing;
                command.ExecutedAtUtc = DateTime.UtcNow;

                bool previousEnabled = state.Enabled;
                int previousInterval =
                    state.SamplingIntervalSeconds;

                bool reversible = true;

                switch (type)
                {
                    case DeviceCommandType.EnableDevice:
                        state.Enabled = true;
                        command.Message = "Device enabled.";
                        break;

                    case DeviceCommandType.DisableDevice:
                        state.Enabled = false;
                        command.Message = "Device disabled.";
                        break;

                    case DeviceCommandType.SetSamplingInterval:
                        state.SamplingIntervalSeconds =
                            request.SamplingIntervalSeconds!.Value;
                        command.Message =
                            "Sampling interval updated.";
                        break;

                    case DeviceCommandType.RestartDevice:
                        reversible = false;
                        command.Message =
                            "Simulated restart completed.";
                        break;

                    case DeviceCommandType.AcknowledgeAlert:
                        reversible = false;
                        command.Message =
                            "Alert acknowledgement recorded.";
                        break;

                    default:
                        command.Status = DeviceCommandStatus.Failed;
                        command.Message = "Unsupported command.";
                        break;
                }

                if (command.Status != DeviceCommandStatus.Failed)
                {
                    command.Status = DeviceCommandStatus.Completed;

                    if (reversible)
                    {
                        _undoStack.Push(new UndoEntry(
                            command.Id,
                            sensorId,
                            previousEnabled,
                            previousInterval));
                    }
                }

                command.CompletedAtUtc = DateTime.UtcNow;

                return Clone(command);
            }
        }

        public Task<IReadOnlyList<DeviceCommand>>
            GetHistoryAsync()
        {
            lock (_lock)
            {
                IReadOnlyList<DeviceCommand> result =
                    _history
                        .OrderByDescending(x => x.CreatedAtUtc)
                        .Select(Clone)
                        .ToList();

                return Task.FromResult(result);
            }
        }

        public Task<DeviceCommand?> GetByIdAsync(Guid id)
        {
            lock (_lock)
            {
                DeviceCommand? command =
                    _history.FirstOrDefault(x => x.Id == id);

                return Task.FromResult(
                    command is null ? null : Clone(command));
            }
        }

        public Task<DeviceCommand?> UndoLastAsync()
        {
            lock (_lock)
            {
                if (_undoStack.Count == 0)
                {
                    return Task.FromResult<DeviceCommand?>(null);
                }

                UndoEntry entry = _undoStack.Pop();

                if (!_states.TryGetValue(
                    entry.SensorId, out DeviceState? state))
                {
                    return Task.FromResult<DeviceCommand?>(null);
                }

                state.Enabled = entry.PreviousEnabled;
                state.SamplingIntervalSeconds =
                    entry.PreviousInterval;

                DeviceCommand? command =
                    _history.FirstOrDefault(
                        x => x.Id == entry.CommandId);

                if (command is null)
                {
                    return Task.FromResult<DeviceCommand?>(null);
                }

                command.Status = DeviceCommandStatus.Undone;
                command.Message = "Command successfully undone.";

                return Task.FromResult<DeviceCommand?>(
                    Clone(command));
            }
        }

        public object GetDeviceState(Guid sensorId)
        {
            lock (_lock)
            {
                if (!_states.TryGetValue(
                    sensorId, out DeviceState? state))
                {
                    return new
                    {
                        sensorId,
                        enabled = true,
                        samplingIntervalSeconds = 30
                    };
                }

                return new
                {
                    sensorId,
                    enabled = state.Enabled,
                    samplingIntervalSeconds =
                        state.SamplingIntervalSeconds
                };
            }
        }

        private static DeviceCommand Clone(
            DeviceCommand command)
        {
            return new DeviceCommand
            {
                Id = command.Id,
                SensorId = command.SensorId,
                Type = command.Type,
                Status = command.Status,
                SamplingIntervalSeconds =
                    command.SamplingIntervalSeconds,
                CreatedAtUtc = command.CreatedAtUtc,
                ExecutedAtUtc = command.ExecutedAtUtc,
                CompletedAtUtc = command.CompletedAtUtc,
                Message = command.Message
            };
        }
    }
}
