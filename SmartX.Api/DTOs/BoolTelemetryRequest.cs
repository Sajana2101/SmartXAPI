using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.DTOs
{
    public class BoolTelemetryRequest
    {
        [Required]
        public Guid SensorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Metric { get; set; } = string.Empty;

        public bool Value { get; set; }
    }
}