namespace SmartX.Api.Models
{
    public class Sensor
    {
        public Guid Id { get; set; }

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string DeploymentLocation { get; set; } = string.Empty;

        public string Room { get; set; } = string.Empty;

        public string Zone { get; set; } = string.Empty;

        public string NodeId { get; set; } = string.Empty;

        public SensorCategory Category { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}