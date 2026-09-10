using SmartX.Api.DTOs;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class TelemetryDashboardService
    {
        private readonly ITelemetryStore
            _telemetryStore;

        private readonly ISensorRepository
            _sensorRepository;

        public TelemetryDashboardService(
            ITelemetryStore telemetryStore,
            ISensorRepository sensorRepository)
        {
            _telemetryStore =
                telemetryStore;

            _sensorRepository =
                sensorRepository;
        }

        public async Task<
            TelemetryDashboardResponse>
            GetDashboardAsync()
        {
            IReadOnlyList<Sensor> sensors =
                await _sensorRepository
                    .GetAllAsync();

            DateTime now =
                DateTime.UtcNow;

            List<TelemetryDashboardSensorResponse>
                results =
                    new();

            foreach (Sensor sensor in sensors)
            {
                TelemetryDashboardSensorResponse
                    response =
                        sensor.Category switch
                        {
                            SensorCategory.Environmental =>
                                BuildFloatResponse(
                                    sensor,
                                    _telemetryStore
                                        .GetFloatPackets(
                                            sensor.Id),
                                    now),

                            SensorCategory.PowerConsumption =>
                                BuildIntResponse(
                                    sensor,
                                    _telemetryStore
                                        .GetIntPackets(
                                            sensor.Id),
                                    now),

                            SensorCategory.Actuator =>
                                BuildBoolResponse(
                                    sensor,
                                    _telemetryStore
                                        .GetBoolPackets(
                                            sensor.Id),
                                    now),

                            _ =>
                                BuildEmptyResponse(
                                    sensor)
                        };

                results.Add(response);
            }

            return new TelemetryDashboardResponse
            {
                GeneratedAtUtc =
                    now,

                TotalSensors =
                    results.Count,

                ConnectedSensors =
                    results.Count(
                        item =>
                            item.IsConnected),

                AnomalyCount =
                    results.Count(
                        item =>
                            item.IsAnomaly),

                Sensors =
                    results
            };
        }

        private static
            TelemetryDashboardSensorResponse
            BuildFloatResponse(
                Sensor sensor,
                IReadOnlyList<
                    TelemetryPacket<float>>
                    packets,
                DateTime now)
        {
            TelemetryPacket<float>? latest =
                packets.FirstOrDefault();

            if (latest is null)
            {
                return BuildEmptyResponse(
                    sensor);
            }

            bool connected =
                IsRecent(
                    latest.TimestampUtc,
                    now);

            bool anomaly =
                IsFloatAnomaly(
                    latest.Metric,
                    latest.Value);

            return new
                TelemetryDashboardSensorResponse
            {
                SensorId =
                    sensor.Id,

                DeviceIdentifier =
                    sensor.DeviceIdentifier,

                Category =
                    sensor.Category.ToString(),

                Metric =
                    latest.Metric,

                LatestValue =
                    latest.Value.ToString("0.0"),

                Unit =
                    GetFloatUnit(
                        latest.Metric),

                LatestTimestampUtc =
                    latest.TimestampUtc,

                IsConnected =
                    connected,

                IsAnomaly =
                    anomaly,

                StatusMessage =
                    GetStatusMessage(
                        connected,
                        anomaly),

                RecentValues =
                    packets
                        .Take(12)
                        .Reverse()
                        .Select(
                            packet =>
                                (double)
                                packet.Value)
                        .ToList()
            };
        }

        private static
            TelemetryDashboardSensorResponse
            BuildIntResponse(
                Sensor sensor,
                IReadOnlyList<
                    TelemetryPacket<int>>
                    packets,
                DateTime now)
        {
            TelemetryPacket<int>? latest =
                packets.FirstOrDefault();

            if (latest is null)
            {
                return BuildEmptyResponse(
                    sensor);
            }

            bool connected =
                IsRecent(
                    latest.TimestampUtc,
                    now);

            bool anomaly =
                latest.Value > 1500;

            return new
                TelemetryDashboardSensorResponse
            {
                SensorId =
                    sensor.Id,

                DeviceIdentifier =
                    sensor.DeviceIdentifier,

                Category =
                    sensor.Category.ToString(),

                Metric =
                    latest.Metric,

                LatestValue =
                    latest.Value.ToString(),

                Unit =
                    "W",

                LatestTimestampUtc =
                    latest.TimestampUtc,

                IsConnected =
                    connected,

                IsAnomaly =
                    anomaly,

                StatusMessage =
                    GetStatusMessage(
                        connected,
                        anomaly),

                RecentValues =
                    packets
                        .Take(12)
                        .Reverse()
                        .Select(
                            packet =>
                                (double)
                                packet.Value)
                        .ToList()
            };
        }

        private static
            TelemetryDashboardSensorResponse
            BuildBoolResponse(
                Sensor sensor,
                IReadOnlyList<
                    TelemetryPacket<bool>>
                    packets,
                DateTime now)
        {
            TelemetryPacket<bool>? latest =
                packets.FirstOrDefault();

            if (latest is null)
            {
                return BuildEmptyResponse(
                    sensor);
            }

            bool connected =
                IsRecent(
                    latest.TimestampUtc,
                    now);

            return new
                TelemetryDashboardSensorResponse
            {
                SensorId =
                    sensor.Id,

                DeviceIdentifier =
                    sensor.DeviceIdentifier,

                Category =
                    sensor.Category.ToString(),

                Metric =
                    latest.Metric,

                LatestValue =
                    latest.Value
                        ? "ON"
                        : "OFF",

                Unit =
                    string.Empty,

                LatestTimestampUtc =
                    latest.TimestampUtc,

                IsConnected =
                    connected,

                IsAnomaly =
                    false,

                StatusMessage =
                    connected
                        ? "Operating normally"
                        : "Sensor disconnected",

                RecentValues =
                    packets
                        .Take(12)
                        .Reverse()
                        .Select(
                            packet =>
                                packet.Value
                                    ? 1.0
                                    : 0.0)
                        .ToList()
            };
        }

        private static
            TelemetryDashboardSensorResponse
            BuildEmptyResponse(
                Sensor sensor)
        {
            return new
                TelemetryDashboardSensorResponse
            {
                SensorId =
                    sensor.Id,

                DeviceIdentifier =
                    sensor.DeviceIdentifier,

                Category =
                    sensor.Category.ToString(),

                Metric =
                    "No telemetry",

                LatestValue =
                    "—",

                IsConnected =
                    false,

                IsAnomaly =
                    false,

                StatusMessage =
                    "No telemetry received"
            };
        }

        private static bool IsRecent(
            DateTime timestamp,
            DateTime now)
        {
            return
                now - timestamp <=
                TimeSpan.FromSeconds(60);
        }

        private static bool IsFloatAnomaly(
            string metric,
            float value)
        {
            if (metric.Contains(
                "temperature",
                StringComparison.OrdinalIgnoreCase))
            {
                return value < 15 ||
                       value > 30;
            }

            if (metric.Contains(
                "moisture",
                StringComparison.OrdinalIgnoreCase))
            {
                return value < 20 ||
                       value > 80;
            }

            return false;
        }

        private static string GetFloatUnit(
            string metric)
        {
            if (metric.Contains(
                "temperature",
                StringComparison.OrdinalIgnoreCase))
            {
                return "°C";
            }

            if (metric.Contains(
                "moisture",
                StringComparison.OrdinalIgnoreCase))
            {
                return "%";
            }

            return string.Empty;
        }

        private static string GetStatusMessage(
            bool connected,
            bool anomaly)
        {
            if (!connected)
            {
                return
                    "Sensor disconnected";
            }

            if (anomaly)
            {
                return
                    "Anomalous telemetry detected";
            }

            return
                "Operating normally";
        }
    }
}