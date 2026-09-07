using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.DTOs
{
    public class FloatTelemetryRequest
    {
        [Required]
        public Guid SensorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Metric { get; set; } = string.Empty;

        public float Value { get; set; }
    }
}