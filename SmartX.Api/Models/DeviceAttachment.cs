namespace SmartX.Api.Models
{
    public class DeviceAttachment
    {
        public Guid Id { get; set; }

        public Guid SensorId { get; set; }

        public string OriginalFileName { get; set; } =
            string.Empty;

        public string ContentType { get; set; } =
            string.Empty;

        public long FileSizeBytes { get; set; }

        public string StorageFileName { get; set; } =
            string.Empty;

        public string IvBase64 { get; set; } =
            string.Empty;

        public DateTime UploadedAtUtc { get; set; }

        public bool EncryptedAtRest { get; set; }
    }
}