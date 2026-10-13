
namespace SmartX.Api.Part2.Models
{
    public enum DeviceIssueType
    {
        Error,
        Disconnected
    }

    public class DeviceIssue
    {
        public Guid SensorId { get; set; }

        public DeviceIssueType Type { get; set; }

        public DateTime FirstDetectedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}
