namespace SmartX.Api.DTOs
{
    public class TelemetryDashboardResponse
    {
        public DateTime GeneratedAtUtc { get; set; }

        public int TotalSensors { get; set; }

        public int ConnectedSensors { get; set; }

        public int AnomalyCount { get; set; }

        public List<TelemetryDashboardSensorResponse>
            Sensors
        { get; set; } =
                new();
    }
}