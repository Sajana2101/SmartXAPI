
namespace SmartX.Api.Part2.Models
{
    public enum AlertSeverity
    {
        Critical = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    public class DeviceAlert
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SensorId { get; set; }

        public AlertSeverity Severity { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}
