namespace SmartX.Api.Models
{
    public class TelemetryPacket<T>
    {
        public Guid Id { get; set; }

        public Guid SensorId { get; set; }

        public string Metric { get; set; } = string.Empty;

        public T Value { get; set; } = default!;

        public DateTime TimestampUtc { get; set; }
    }
}