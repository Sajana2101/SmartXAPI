using System.ComponentModel.DataAnnotations;
using SmartX.Api.Models;

namespace SmartX.Api.DTOs
{
    public class UpdateSensorRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string DeviceIdentifier { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string DeploymentLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Room { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Zone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NodeId { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(SensorCategory))]
        public SensorCategory Category { get; set; }
    }
}