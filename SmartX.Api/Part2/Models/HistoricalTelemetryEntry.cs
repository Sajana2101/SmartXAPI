
namespace SmartX.Api.Part2.Models
{
    public class HistoricalTelemetryEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SensorId { get; set; }

        public string Metric { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string ValueType { get; set; } = string.Empty;

        public DateTime TimestampUtc { get; set; }
    }
}
