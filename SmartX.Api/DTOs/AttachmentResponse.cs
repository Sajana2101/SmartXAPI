namespace SmartX.Api.DTOs
{
    public class AttachmentResponse
    {
        public Guid Id { get; set; }

        public Guid SensorId { get; set; }

        public string FileName { get; set; } =
            string.Empty;

        public string ContentType { get; set; } =
            string.Empty;

        public long FileSizeBytes { get; set; }

        public DateTime UploadedAtUtc { get; set; }

        public bool EncryptedAtRest { get; set; }
    }
}