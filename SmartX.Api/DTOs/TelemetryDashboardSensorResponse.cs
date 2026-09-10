namespace SmartX.Api.DTOs
{
    public class TelemetryDashboardSensorResponse
    {
        public Guid SensorId { get; set; }

        public string DeviceIdentifier { get; set; } =
            string.Empty;

        public string Category { get; set; } =
            string.Empty;

        public string Metric { get; set; } =
            string.Empty;

        public string LatestValue { get; set; } =
            string.Empty;

        public string Unit { get; set; } =
            string.Empty;

        public DateTime? LatestTimestampUtc { get; set; }

        public bool IsConnected { get; set; }

        public bool IsAnomaly { get; set; }

        public string StatusMessage { get; set; } =
            string.Empty;

        public List<double> RecentValues { get; set; } =
            new();
    }
}