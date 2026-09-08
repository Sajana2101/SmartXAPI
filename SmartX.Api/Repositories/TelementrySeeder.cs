using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class TelemetrySeeder
    {
        private readonly ITelemetryStore _telemetryStore;
        private readonly ISensorRepository _sensorRepository;

        public TelemetrySeeder(
            ITelemetryStore telemetryStore,
            ISensorRepository sensorRepository)
        {
            _telemetryStore = telemetryStore;
            _sensorRepository = sensorRepository;
        }

        public async Task<int> SeedAsync(int readingsPerSensor)
        {
            IReadOnlyList<Sensor> sensors =
                await _sensorRepository.GetAllAsync();

            Random random = new();

            int created = 0;

            foreach (Sensor sensor in sensors)
            {
                for (int i = 0; i < readingsPerSensor; i++)
                {
                    DateTime timestamp =
                        DateTime.UtcNow.AddSeconds(-i);

                    switch (sensor.Category)
                    {
                        case SensorCategory.Environmental:
                            _telemetryStore.AddFloat(
                                new TelemetryPacket<float>
                                {
                                    Id = Guid.NewGuid(),
                                    SensorId = sensor.Id,
                                    Metric = "Temperature",
                                    Value =
                                        (float)(18 +
                                        random.NextDouble() * 18),
                                    TimestampUtc = timestamp
                                });

                            created++;
                            break;

                        case SensorCategory.PowerConsumption:
                            _telemetryStore.AddInt(
                                new TelemetryPacket<int>
                                {
                                    Id = Guid.NewGuid(),
                                    SensorId = sensor.Id,
                                    Metric = "PowerWatts",
                                    Value = random.Next(100, 2000),
                                    TimestampUtc = timestamp
                                });

                            created++;
                            break;

                        case SensorCategory.Actuator:
                            _telemetryStore.AddBool(
                                new TelemetryPacket<bool>
                                {
                                    Id = Guid.NewGuid(),
                                    SensorId = sensor.Id,
                                    Metric = "ValveState",
                                    Value = random.Next(0, 2) == 1,
                                    TimestampUtc = timestamp
                                });

                            created++;
                            break;
                    }
                }
            }

            return created;
        }
    }
}