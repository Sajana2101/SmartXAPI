namespace SmartX.Api.Options
{
    public class AttachmentOptions
    {
        public const string SectionName = "Attachments";

        public string StoragePath { get; set; } =
            "App_Data/Attachments";

        public long MaxFileSizeBytes { get; set; } =
            10 * 1024 * 1024;

        public string[] AllowedExtensions { get; set; } =
            Array.Empty<string>();

        public string EncryptionKey { get; set; } =
            string.Empty;
    }
}