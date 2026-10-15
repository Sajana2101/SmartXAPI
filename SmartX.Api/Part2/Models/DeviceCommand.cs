
namespace SmartX.Api.Part2.Models
{
    public class DeviceCommand
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SensorId { get; set; }

        public DeviceCommandType Type { get; set; }

        public DeviceCommandStatus Status { get; set; }
            = DeviceCommandStatus.Queued;

        public int? SamplingIntervalSeconds { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? ExecutedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public string? Message { get; set; }
    }
}
