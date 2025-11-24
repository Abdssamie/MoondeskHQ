using System.Security.Cryptography;
using System.Text;
using Moondesk.Domain.Interfaces.Services;

namespace Moondesk.BackgroundServices.Services;

public class EncryptionService : IEncryptionService
{
    // In production, this should come from a secure Key Vault or Environment Variable
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        // Use a default key for dev if not in config, ensuring 32 bytes for AES-256
        var keyString = configuration["Security:EncryptionKey"] ?? "A_VERY_STRONG_32_BYTE_KEY_FOR_AES256!";
        _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
    }

    public (string Encrypted, string IV) Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return (string.Empty, string.Empty);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plaintext);
        }

        return (Convert.ToBase64String(ms.ToArray()), Convert.ToBase64String(aes.IV));
    }

    public string Decrypt(string cipherTextBase64, string ivBase64)
    {
        if (string.IsNullOrEmpty(cipherTextBase64) || string.IsNullOrEmpty(ivBase64)) return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Convert.FromBase64String(ivBase64);

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(cipherTextBase64));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
