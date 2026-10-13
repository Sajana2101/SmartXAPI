
using System.ComponentModel.DataAnnotations;
using SmartX.Api.Part2.Models;

namespace SmartX.Api.Part2.DTOs
{
    public class CreateDeviceCommandRequest
    {
        [Required]
        public Guid? SensorId { get; set; }

        [Required]
        [EnumDataType(typeof(DeviceCommandType))]
        public DeviceCommandType? Type { get; set; }

        [Range(1, 86400)]
        public int? SamplingIntervalSeconds { get; set; }
    }
}
