
using System.Net.Http.Json;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class TelemetrySeeder
    {
        private readonly ISensorRepository _sensors;
        private readonly HttpClient _client = new();

        public TelemetrySeeder(ISensorRepository sensors)
        {
            _sensors = sensors;
        }

        public async Task<int> SeedAsync(
            int readingsPerSensor,
            string apiBaseUrl,
            CancellationToken cancellationToken = default)
        {
            if (readingsPerSensor < 1 || readingsPerSensor > 10000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(readingsPerSensor));
            }

            if (!Uri.TryCreate(
                apiBaseUrl,
                UriKind.Absolute,
                out Uri? baseUri) ||
                !baseUri.IsLoopback ||
                (baseUri.Scheme != Uri.UriSchemeHttps &&
                 baseUri.Scheme != Uri.UriSchemeHttp))
            {
                throw new ArgumentException(
                    "The simulation API URL must be a local HTTP endpoint.");
            }

            IReadOnlyList<Sensor> sensors =
                await _sensors.GetAllAsync();

            int created = 0;

            foreach (Sensor sensor in sensors)
            {
                for (int i = 0; i < readingsPerSensor; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string endpoint;
                    object payload;

                    switch (sensor.Category)
                    {
                        case SensorCategory.Environmental:
                            endpoint = "/api/telemetry/float";
                            payload = new
                            {
                                sensorId = sensor.Id,
                                metric = "temperature",
                                value = Random.Shared.Next(180, 361) / 10.0
                            };
                            break;

                        case SensorCategory.PowerConsumption:
                            endpoint = "/api/telemetry/int";
                            payload = new
                            {
                                sensorId = sensor.Id,
                                metric = "PowerWatts",
                                value = Random.Shared.Next(100, 2001)
                            };
                            break;

                        case SensorCategory.Actuator:
                            endpoint = "/api/telemetry/bool";
                            payload = new
                            {
                                sensorId = sensor.Id,
                                metric = "ValveState",
                                value = Random.Shared.Next(0, 2) == 1
                            };
                            break;

                        default:
                            continue;
                    }

                    Uri destination = new(baseUri, endpoint);

                    using HttpResponseMessage response =
                        await _client.PostAsJsonAsync(
                            destination,
                            payload,
                            cancellationToken);

                    response.EnsureSuccessStatusCode();

                    created++;
                }
            }

            return created;
        }
    }
}
