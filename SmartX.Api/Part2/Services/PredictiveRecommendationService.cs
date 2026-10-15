
using SmartX.Api.Part2.Interfaces;
using SmartX.Api.Part2.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

namespace SmartX.Api.Part2.Services
{
    public class PredictiveRecommendationService
    {
        private readonly ISensorRepository _sensors;
        private readonly TelemetryDashboardService _dashboard;
        private readonly DeviceBehaviorAnalysisService _analysis;
        private readonly DeviceSearchHistoryService _searches;
        private readonly IDeviceCommandService _commands;
        private readonly DeviceIssueTracker _issues;

        public PredictiveRecommendationService(
            ISensorRepository sensors,
            TelemetryDashboardService dashboard,
            DeviceBehaviorAnalysisService analysis,
            DeviceSearchHistoryService searches,
            IDeviceCommandService commands,
            DeviceIssueTracker issues)
        {
            _sensors = sensors;
            _dashboard = dashboard;
            _analysis = analysis;
            _searches = searches;
            _commands = commands;
            _issues = issues;
        }

        public async Task<PredictiveInsightsResponse> GenerateAsync()
        {
            var sensors = await _sensors.GetAllAsync();
            var dashboard = await _dashboard.GetDashboardAsync();
            var behavior = await _analysis.AnalyseAsync(
                sensors.Select(x => x.Id));

            var activeIssues = _issues.GetActive();
            var commandHistory = await _commands.GetHistoryAsync();

            DateTime now = DateTime.UtcNow;
            List<PredictiveRecommendation> recommendations = new();

            foreach (var sensor in sensors)
            {
                var reading = dashboard.Sensors.FirstOrDefault(
                    x => x.SensorId == sensor.Id);

                var activity = behavior.First(
                    x => x.SensorId == sensor.Id);

                if (reading is null)
                {
                    continue;
                }

                bool hasTelemetry = reading.LatestTimestampUtc.HasValue;

                bool disconnected =
                    hasTelemetry && !reading.IsConnected;

                bool activeDisconnect = activeIssues.Any(
                    x => x.SensorId == sensor.Id &&
                         x.Type == DeviceIssueType.Disconnected);

                bool activeError = activeIssues.Any(
                    x => x.SensorId == sensor.Id &&
                         x.Type == DeviceIssueType.Error);

                int score = 0;
                List<string> evidence = new();

                if (disconnected || activeDisconnect)
                {
                    score += 65;
                    evidence.Add("Device disconnection detected");
                }

                if (reading.IsAnomaly || activeError)
                {
                    score += 45;
                    evidence.Add("Current sensor anomaly or active error");
                }

                if (activity.ConsecutiveAnomalousReadings >= 2)
                {
                    score += 20;
                    evidence.Add(
                        $"{activity.ConsecutiveAnomalousReadings} consecutive anomalous historical readings");
                }

                if (activity.SearchCount >= 3)
                {
                    score += 12;
                    evidence.Add(
                        $"{activity.SearchCount} device searches in seven days");
                }

                if (activity.SearchCommandCorrelations >= 2)
                {
                    score += 20;
                    evidence.Add(
                        $"{activity.SearchCommandCorrelations} searches followed by manual commands");
                }

                if (activity.CommandCount >= 3)
                {
                    score += 8;
                    evidence.Add(
                        $"{activity.CommandCount} recent manual commands");
                }

                if (score == 0)
                {
                    continue;
                }

                score = Math.Min(score, 100);

                string priority = score >= 65
                    ? "Critical"
                    : score >= 35
                        ? "High"
                        : "Advisory";

                string title;
                string explanation;
                string suggestedCommand;

                if (disconnected || activeDisconnect)
                {
                    title = "Investigate disconnected device";
                    explanation =
                        "The device is not reporting normally. Check network connectivity and power before attempting a restart.";
                    suggestedCommand = "RestartDevice";
                }
                else if (reading.IsAnomaly || activeError ||
                         activity.ConsecutiveAnomalousReadings >= 2)
                {
                    title = "Investigate abnormal sensor readings";
                    explanation =
                        "Sensor data indicates a potentially unsafe or abnormal operating condition. Inspect the device and its environment.";
                    suggestedCommand = "AcknowledgeAlert";
                }
                else
                {
                    title = "Review recurring device intervention";
                    explanation =
                        "Repeated device searches and manual commands suggest an issue that may need investigation.";
                    suggestedCommand = "InspectDevice";
                }

                recommendations.Add(new PredictiveRecommendation
                {
                    Id = $"insight-{sensor.Id:N}",
                    SensorId = sensor.Id,
                    DeviceIdentifier = sensor.DeviceIdentifier,
                    Title = title,
                    Explanation = explanation,
                    SuggestedCommand = suggestedCommand,
                    Score = score,
                    Priority = priority,
                    Evidence = string.Join("; ", evidence),
                    GeneratedAtUtc = now
                });
            }

            return new PredictiveInsightsResponse
            {
                GeneratedAtUtc = now,
                AnalysedSensorCount = sensors.Count,
                RecordedSearchCount = _searches.Count,
                RecordedCommandCount = commandHistory.Count,
                Recommendations = recommendations
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.DeviceIdentifier)
                    .ToList(),
                Behavior = behavior
            };
        }
    }
}
