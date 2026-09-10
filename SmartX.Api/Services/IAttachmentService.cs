using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public interface IAttachmentService
    {
        Task<DeviceAttachment> UploadAsync(
            Guid sensorId,
            IFormFile file,
            CancellationToken cancellationToken);

        IReadOnlyList<DeviceAttachment> GetBySensor(
            Guid sensorId);

        DeviceAttachment? GetById(Guid id);

        Task<byte[]> DownloadAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task<bool> DeleteAsync(Guid id);
    }
}