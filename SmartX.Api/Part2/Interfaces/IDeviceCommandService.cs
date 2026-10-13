
using SmartX.Api.Part2.DTOs;
using SmartX.Api.Part2.Models;

namespace SmartX.Api.Part2.Interfaces
{
    public interface IDeviceCommandService
    {
        Task<DeviceCommand> SubmitAsync(
            CreateDeviceCommandRequest request);

        Task<IReadOnlyList<DeviceCommand>> GetHistoryAsync();

        Task<DeviceCommand?> GetByIdAsync(Guid id);

        Task<DeviceCommand?> UndoLastAsync();
    }
}
