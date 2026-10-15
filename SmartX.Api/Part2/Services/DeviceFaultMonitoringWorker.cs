
using SmartX.Api.Part2.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

namespace SmartX.Api.Part2.Services
{
    public class DeviceFaultMonitoringWorker : BackgroundService
    {
        private readonly TelemetryDashboardService _dashboard;
        private readonly DeviceIssueTracker _issues;
        private readonly PriorityAlertService _alerts;
        private readonly ILogger<DeviceFaultMonitoringWorker> _logger;

        public DeviceFaultMonitoringWorker(
            TelemetryDashboardService dashboard,
            DeviceIssueTracker issues,
            PriorityAlertService alerts,
            ILogger<DeviceFaultMonitoringWorker> logger)
        {
            _dashboard = dashboard;
            _issues = issues;
            _alerts = alerts;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            using PeriodicTimer timer =
                new(TimeSpan.FromSeconds(5));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Automatic device fault monitoring failed.");
                }

                try
                {
                    if (!await timer.WaitForNextTickAsync(
                        stoppingToken))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task CheckAsync()
        {
            var dashboard = await _dashboard.GetDashboardAsync();

            foreach (var sensor in dashboard.Sensors)
            {
                if (!sensor.LatestTimestampUtc.HasValue)
                {
                    continue;
                }

                if (!sensor.IsConnected)
                {
                    bool added = _issues.Report(
                        sensor.SensorId,
                        DeviceIssueType.Disconnected);

                    if (added)
                    {
                        _alerts.Enqueue(new DeviceAlert
                        {
                            SensorId = sensor.SensorId,
                            Severity = AlertSeverity.Critical,
                            Message =
                                $"Device {sensor.DeviceIdentifier} disconnected."
                        });
                    }
                }
                else
                {
                    _issues.Resolve(
                        sensor.SensorId,
                        DeviceIssueType.Disconnected);
                }

                if (sensor.IsAnomaly)
                {
                    bool added = _issues.Report(
                        sensor.SensorId,
                        DeviceIssueType.Error);

                    if (added)
                    {
                        _alerts.Enqueue(new DeviceAlert
                        {
                            SensorId = sensor.SensorId,
                            Severity = AlertSeverity.High,
                            Message =
                                $"Abnormal telemetry from {sensor.DeviceIdentifier}: {sensor.Metric} = {sensor.LatestValue}{sensor.Unit}"
                        });
                    }
                }
                else
                {
                    _issues.Resolve(
                        sensor.SensorId,
                        DeviceIssueType.Error);
                }
            }
        }
    }
}
