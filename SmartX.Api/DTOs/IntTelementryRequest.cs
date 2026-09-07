using System.ComponentModel.DataAnnotations;

namespace SmartX.Api.DTOs
{
    public class IntTelemetryRequest
    {
        [Required]
        public Guid SensorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Metric { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}