
using SmartX.Api.Part2.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Part2.Services
{
    public class DeviceFaultReconciliationService
    {
        private readonly TelemetryDashboardService _dashboard;
        private readonly DeviceIssueTracker _tracker;

        public DeviceFaultReconciliationService(
            TelemetryDashboardService dashboard,
            DeviceIssueTracker tracker)
        {
            _dashboard = dashboard;
            _tracker = tracker;
        }

        public async Task<FaultReconciliationResult> ReconcileAsync()
        {
            var dashboard = await _dashboard.GetDashboardAsync();

            int newErrors = 0;
            int newDisconnections = 0;
            int resolvedIssues = 0;

            foreach (var sensor in dashboard.Sensors)
            {
                bool hasTelemetry =
                    sensor.LatestTimestampUtc.HasValue;

                if (!hasTelemetry)
                {
                    continue;
                }

                if (sensor.IsAnomaly)
                {
                    if (_tracker.ReportAutomatic(
                        sensor.SensorId,
                        DeviceIssueType.Error))
                    {
                        newErrors++;
                    }
                }
                else if (_tracker.ResolveAutomatic(
                    sensor.SensorId,
                    DeviceIssueType.Error))
                {
                    resolvedIssues++;
                }

                if (!sensor.IsConnected)
                {
                    if (_tracker.ReportAutomatic(
                        sensor.SensorId,
                        DeviceIssueType.Disconnected))
                    {
                        newDisconnections++;
                    }
                }
                else if (_tracker.ResolveAutomatic(
                    sensor.SensorId,
                    DeviceIssueType.Disconnected))
                {
                    resolvedIssues++;
                }
            }

            return new FaultReconciliationResult
            {
                SensorsChecked = dashboard.Sensors.Count,
                NewErrors = newErrors,
                NewDisconnections = newDisconnections,
                ResolvedIssues = resolvedIssues,
                ActiveIssues = _tracker.ActiveCount
            };
        }
    }

    public class FaultReconciliationResult
    {
        public int SensorsChecked { get; set; }

        public int NewErrors { get; set; }

        public int NewDisconnections { get; set; }

        public int ResolvedIssues { get; set; }

        public int ActiveIssues { get; set; }
    }
}
