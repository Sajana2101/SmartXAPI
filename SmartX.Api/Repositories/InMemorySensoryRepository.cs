
using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public class InMemorySensorRepository : ISensorRepository
    {
        private readonly Dictionary<Guid, Sensor> _sensors = new();

        private readonly Dictionary<string, Guid> _deviceIdentifiers =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _lock = new();

        public Task<IReadOnlyList<Sensor>> GetAllAsync()
        {
            lock (_lock)
            {
                IReadOnlyList<Sensor> sensors = _sensors.Values
                    .OrderByDescending(sensor => sensor.CreatedAtUtc)
                    .Select(Clone)
                    .ToList();

                return Task.FromResult(sensors);
            }
        }

        public Task<Sensor?> GetByIdAsync(Guid id)
        {
            lock (_lock)
            {
                return Task.FromResult(
                    _sensors.TryGetValue(id, out Sensor? sensor)
                        ? Clone(sensor)
                        : null);
            }
        }

        public Task<Sensor?> GetByDeviceIdentifierAsync(
            string deviceIdentifier)
        {
            lock (_lock)
            {
                if (!_deviceIdentifiers.TryGetValue(
                    deviceIdentifier, out Guid id))
                {
                    return Task.FromResult<Sensor?>(null);
                }

                return Task.FromResult<Sensor?>(
                    _sensors.TryGetValue(id, out Sensor? sensor)
                        ? Clone(sensor)
                        : null);
            }
        }

        public Task AddAsync(Sensor sensor)
        {
            lock (_lock)
            {
                if (_sensors.ContainsKey(sensor.Id) ||
                    _deviceIdentifiers.ContainsKey(
                        sensor.DeviceIdentifier))
                {
                    throw new InvalidOperationException(
                        "Sensor ID or device identifier already exists.");
                }

                Sensor stored = Clone(sensor);

                _sensors.Add(stored.Id, stored);

                _deviceIdentifiers.Add(
                    stored.DeviceIdentifier, stored.Id);
            }

            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Sensor sensor)
        {
            lock (_lock)
            {
                if (!_sensors.TryGetValue(
                    sensor.Id, out Sensor? existing))
                {
                    return Task.FromResult(false);
                }

                if (_deviceIdentifiers.TryGetValue(
                    sensor.DeviceIdentifier, out Guid ownerId) &&
                    ownerId != sensor.Id)
                {
                    throw new InvalidOperationException(
                        "Device identifier already belongs to another sensor.");
                }

                _deviceIdentifiers.Remove(
                    existing.DeviceIdentifier);

                Sensor stored = Clone(sensor);

                _sensors[sensor.Id] = stored;

                _deviceIdentifiers[stored.DeviceIdentifier] =
                    stored.Id;

                return Task.FromResult(true);
            }
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            lock (_lock)
            {
                if (!_sensors.Remove(id, out Sensor? removed))
                {
                    return Task.FromResult(false);
                }

                _deviceIdentifiers.Remove(
                    removed.DeviceIdentifier);

                return Task.FromResult(true);
            }
        }

        private static Sensor Clone(Sensor sensor)
        {
            return new Sensor
            {
                Id = sensor.Id,
                DeviceIdentifier = sensor.DeviceIdentifier,
                DeploymentLocation = sensor.DeploymentLocation,
                Room = sensor.Room,
                Zone = sensor.Zone,
                NodeId = sensor.NodeId,
                Category = sensor.Category,
                CreatedAtUtc = sensor.CreatedAtUtc,
                UpdatedAtUtc = sensor.UpdatedAtUtc
            };
        }
    }
}
