
using System.Globalization;
using SmartX.Api.Part2.Interfaces;
using SmartX.Api.Part2.Models;

namespace SmartX.Api.Part2.Services
{
    public class DeviceBehaviorAnalysisService
    {
        private readonly DeviceSearchHistoryService _searches;

        private readonly IDeviceCommandService _commands;

        private readonly SortedTelemetryHistoryService _telemetry;

        public DeviceBehaviorAnalysisService(
            DeviceSearchHistoryService searches,
            IDeviceCommandService commands,
            SortedTelemetryHistoryService telemetry)
        {
            _searches = searches;
            _commands = commands;
            _telemetry = telemetry;
        }

        public async Task<List<DeviceBehaviorSummary>>
            AnalyseAsync(IEnumerable<Guid> sensorIds)
        {
            DateTime cutoff = DateTime.UtcNow.AddDays(-7);

            var searches = _searches.GetHistory()
                .Where(x => x.SearchedAtUtc >= cutoff)
                .ToList();

            var commands = (await _commands.GetHistoryAsync())
                .Where(x => x.CreatedAtUtc >= cutoff)
                .ToList();

            var telemetry = _telemetry.GetHistory()
                .Where(x => x.TimestampUtc >= cutoff)
                .ToList();

            List<DeviceBehaviorSummary> results = new();

            foreach (Guid sensorId in sensorIds.Distinct())
            {
                var sensorSearches = searches
                    .Where(x => x.SensorId == sensorId)
                    .ToList();

                var sensorCommands = commands
                    .Where(x => x.SensorId == sensorId)
                    .ToList();

                var sensorTelemetry = telemetry
                    .Where(x => x.SensorId == sensorId)
                    .OrderByDescending(x => x.TimestampUtc)
                    .Take(20)
                    .ToList();

                int correlations = 0;

                foreach (var search in sensorSearches)
                {
                    bool followedByCommand = sensorCommands.Any(
                        command =>
                            command.CreatedAtUtc >= search.SearchedAtUtc &&
                            command.CreatedAtUtc <=
                            search.SearchedAtUtc.AddMinutes(10));

                    if (followedByCommand)
                    {
                        correlations++;
                    }
                }

                int consecutiveAnomalies = 0;

                foreach (var reading in sensorTelemetry)
                {
                    if (!IsAnomalous(reading))
                    {
                        break;
                    }

                    consecutiveAnomalies++;
                }

                DateTime? lastInteraction = sensorSearches
                    .Select(x => (DateTime?)x.SearchedAtUtc)
                    .Concat(sensorCommands.Select(
                        x => (DateTime?)x.CreatedAtUtc))
                    .Max();

                results.Add(new DeviceBehaviorSummary
                {
                    SensorId = sensorId,
                    SearchCount = sensorSearches.Count,
                    CommandCount = sensorCommands.Count,
                    RecentTelemetryCount = sensorTelemetry.Count,
                    ConsecutiveAnomalousReadings =
                        consecutiveAnomalies,
                    SearchCommandCorrelations = correlations,
                    LastInteractionUtc = lastInteraction
                });
            }

            return results;
        }

        private static bool IsAnomalous(
            HistoricalTelemetryEntry reading)
        {
            if (!double.TryParse(
                reading.Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value))
            {
                return false;
            }

            string metric = reading.Metric.ToLowerInvariant();

            if (metric.Contains("moisture") ||
                metric.Contains("humidity"))
            {
                return value < 20 || value > 90;
            }

            if (metric.Contains("temperature"))
            {
                return value < 0 || value > 45;
            }

            if (metric.Contains("power") ||
                metric.Contains("watt"))
            {
                return value > 1500;
            }

            return false;
        }
    }
}
