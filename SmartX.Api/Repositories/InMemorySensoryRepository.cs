using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public class InMemorySensorRepository : ISensorRepository
    {
        private readonly List<Sensor> _sensors = new();
        private readonly object _lock = new();

        public Task<IReadOnlyList<Sensor>> GetAllAsync()
        {
            lock (_lock)
            {
                IReadOnlyList<Sensor> sensors = _sensors
                    .OrderByDescending(sensor => sensor.CreatedAtUtc)
                    .ToList();

                return Task.FromResult(sensors);
            }
        }

        public Task<Sensor?> GetByIdAsync(Guid id)
        {
            lock (_lock)
            {
                Sensor? sensor = _sensors.FirstOrDefault(sensor => sensor.Id == id);

                return Task.FromResult(sensor);
            }
        }

        public Task<Sensor?> GetByDeviceIdentifierAsync(string deviceIdentifier)
        {
            lock (_lock)
            {
                Sensor? sensor = _sensors.FirstOrDefault(sensor =>
                    sensor.DeviceIdentifier.Equals(
                        deviceIdentifier,
                        StringComparison.OrdinalIgnoreCase));

                return Task.FromResult(sensor);
            }
        }

        public Task AddAsync(Sensor sensor)
        {
            lock (_lock)
            {
                _sensors.Add(sensor);
            }

            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Sensor sensor)
        {
            lock (_lock)
            {
                int index = _sensors.FindIndex(existing => existing.Id == sensor.Id);

                if (index == -1)
                {
                    return Task.FromResult(false);
                }

                _sensors[index] = sensor;

                return Task.FromResult(true);
            }
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            lock (_lock)
            {
                Sensor? sensor = _sensors.FirstOrDefault(sensor => sensor.Id == id);

                if (sensor is null)
                {
                    return Task.FromResult(false);
                }

                _sensors.Remove(sensor);

                return Task.FromResult(true);
            }
        }
    }
}