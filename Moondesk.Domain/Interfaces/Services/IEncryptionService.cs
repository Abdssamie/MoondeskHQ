namespace Moondesk.Domain.Interfaces.Services;

public interface IEncryptionService
{
    // private readonly byte[] Key = Encoding.UTF8.GetBytes("A_VERY_STRONG_32_BYTE_KEY_FOR_AES256!"); 
    public (string Encrypted, string IV) Encrypt(string plaintext);

    public string Decrypt(string cipherTextBase64, string ivBase64);
}