using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SmartX.Api.Options;

namespace SmartX.Api.Services
{
    public class AesFileEncryptionService :
        IFileEncryptionService
    {
        private readonly byte[] _key;

        public AesFileEncryptionService(
            IOptions<AttachmentOptions> options)
        {
            string configuredKey =
                options.Value.EncryptionKey;

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                throw new InvalidOperationException(
                    "Attachment encryption key has not been configured.");
            }

            _key = Convert.FromBase64String(configuredKey);

            if (_key.Length != 32)
            {
                throw new InvalidOperationException(
                    "Attachment encryption key must be 256 bits.");
            }
        }

        public async Task<string> EncryptAsync(
            Stream input,
            string outputPath,
            CancellationToken cancellationToken)
        {
            using Aes aes = Aes.Create();

            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateIV();

            await using FileStream output =
                new(
                    outputPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true);

            using ICryptoTransform encryptor =
                aes.CreateEncryptor();

            await using CryptoStream cryptoStream =
                new(
                    output,
                    encryptor,
                    CryptoStreamMode.Write,
                    leaveOpen: false);

            await input.CopyToAsync(
                cryptoStream,
                81920,
                cancellationToken);

            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(aes.IV);
        }

        public async Task<byte[]> DecryptAsync(
            string inputPath,
            string ivBase64,
            CancellationToken cancellationToken)
        {
            byte[] iv =
                Convert.FromBase64String(ivBase64);

            using Aes aes = Aes.Create();

            aes.Key = _key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            await using FileStream input =
                new(
                    inputPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    useAsync: true);

            using ICryptoTransform decryptor =
                aes.CreateDecryptor();

            await using CryptoStream cryptoStream =
                new(
                    input,
                    decryptor,
                    CryptoStreamMode.Read);

            await using MemoryStream output =
                new();

            await cryptoStream.CopyToAsync(
                output,
                81920,
                cancellationToken);

            return output.ToArray();
        }
    }
}