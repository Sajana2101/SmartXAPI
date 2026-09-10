namespace SmartX.Api.Services
{
    public interface IFileEncryptionService
    {
        Task<string> EncryptAsync(
            Stream input,
            string outputPath,
            CancellationToken cancellationToken);

        Task<byte[]> DecryptAsync(
            string inputPath,
            string ivBase64,
            CancellationToken cancellationToken);
    }
}