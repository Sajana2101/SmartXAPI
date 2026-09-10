using Microsoft.Extensions.Options;
using SmartX.Api.Models;
using SmartX.Api.Options;

namespace SmartX.Api.Services
{
    public class AttachmentService :
        IAttachmentService
    {
        private readonly List<DeviceAttachment> _attachments =
            new();

        private readonly object _lock =
            new();

        private readonly string _storagePath;

        private readonly AttachmentOptions _options;

        private readonly IFileEncryptionService
            _encryptionService;

        public AttachmentService(
            IWebHostEnvironment environment,
            IOptions<AttachmentOptions> options,
            IFileEncryptionService encryptionService)
        {
            _options = options.Value;

            _encryptionService =
                encryptionService;

            _storagePath =
                Path.Combine(
                    environment.ContentRootPath,
                    _options.StoragePath);

            Directory.CreateDirectory(
                _storagePath);
        }

        public async Task<DeviceAttachment> UploadAsync(
            Guid sensorId,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            if (file.Length == 0)
            {
                throw new InvalidOperationException(
                    "The uploaded file is empty.");
            }

            if (file.Length >
                _options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    "The uploaded file exceeds the 10 MB limit.");
            }

            string extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            bool allowed =
                _options.AllowedExtensions.Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase);

            if (!allowed)
            {
                throw new InvalidOperationException(
                    "This file type is not supported.");
            }

            string safeOriginalName =
                Path.GetFileName(file.FileName);

            Guid attachmentId =
                Guid.NewGuid();

            string storageFileName =
                $"{attachmentId:N}.bin";

            string fullPath =
                Path.Combine(
                    _storagePath,
                    storageFileName);

            await using Stream input =
                file.OpenReadStream();

            string ivBase64 =
                await _encryptionService.EncryptAsync(
                    input,
                    fullPath,
                    cancellationToken);

            DeviceAttachment attachment =
                new()
                {
                    Id = attachmentId,
                    SensorId = sensorId,
                    OriginalFileName =
                        safeOriginalName,
                    ContentType =
                        string.IsNullOrWhiteSpace(
                            file.ContentType)
                            ? "application/octet-stream"
                            : file.ContentType,
                    FileSizeBytes =
                        file.Length,
                    StorageFileName =
                        storageFileName,
                    IvBase64 =
                        ivBase64,
                    UploadedAtUtc =
                        DateTime.UtcNow,
                    EncryptedAtRest =
                        true
                };

            lock (_lock)
            {
                _attachments.Add(
                    attachment);
            }

            return attachment;
        }

        public IReadOnlyList<DeviceAttachment>
            GetBySensor(Guid sensorId)
        {
            lock (_lock)
            {
                return _attachments
                    .Where(
                        attachment =>
                            attachment.SensorId ==
                            sensorId)
                    .OrderByDescending(
                        attachment =>
                            attachment.UploadedAtUtc)
                    .ToList();
            }
        }

        public DeviceAttachment? GetById(
            Guid id)
        {
            lock (_lock)
            {
                return _attachments
                    .FirstOrDefault(
                        attachment =>
                            attachment.Id == id);
            }
        }

        public async Task<byte[]> DownloadAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            DeviceAttachment? attachment =
                GetById(id);

            if (attachment is null)
            {
                throw new FileNotFoundException(
                    "Attachment could not be found.");
            }

            string fullPath =
                Path.Combine(
                    _storagePath,
                    attachment.StorageFileName);

            return await
                _encryptionService.DecryptAsync(
                    fullPath,
                    attachment.IvBase64,
                    cancellationToken);
        }

        public Task<bool> DeleteAsync(
            Guid id)
        {
            DeviceAttachment? attachment;

            lock (_lock)
            {
                attachment =
                    _attachments
                        .FirstOrDefault(
                            item =>
                                item.Id == id);

                if (attachment is null)
                {
                    return Task.FromResult(false);
                }

                _attachments.Remove(
                    attachment);
            }

            string fullPath =
                Path.Combine(
                    _storagePath,
                    attachment.StorageFileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.FromResult(true);
        }
    }
}