# Moondesk TODO

## 🔴 Critical Security Issues

### Implement ConnectionCredential Encryption
**Priority:** HIGH  
**Status:** Not Implemented

**Problem:**
- `ConnectionCredential` model has `EncryptedPassword` and `EncryptionIV` fields
- `IEncryptionService` interface exists but has NO implementation
- Credentials (MQTT passwords, database passwords) are currently stored in plain text

**Required Implementation:**

1. **Create AES Encryption Service**
   - Location: `Moondesk.DataAccess/Services/EncryptionService.cs`
   - Implement `IEncryptionService` interface
   - Use AES-256-CBC encryption
   - Generate random IV per encryption

2. **Secure Key Management**
   - Store encryption key in environment variable (dev)
   - Use Azure Key Vault or AWS Secrets Manager (production)
   - Never commit encryption keys to git

3. **Update ConnectionCredentialRepository**
   - Encrypt password before saving to database
   - Decrypt password when retrieving from database
   - Use `IEncryptionService` in constructor

4. **Add Configuration**
   ```json
   {
     "Encryption": {
       "Key": "your-32-byte-base64-key-here"
     }
   }
   ```

**Example Implementation:**
```csharp
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"] 
            ?? throw new InvalidOperationException("Encryption key not configured");
        _key = Convert.FromBase64String(keyString);
    }

    public (string encryptedData, string iv) Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        return (Convert.ToBase64String(encrypted), Convert.ToBase64String(aes.IV));
    }

    public string Decrypt(string encryptedData, string iv)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Convert.FromBase64String(iv);
        
        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(encryptedData);
        var decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        
        return Encoding.UTF8.GetString(decrypted);
    }
}
```

**Testing:**
- [ ] Unit tests for encryption/decryption
- [ ] Verify different IVs for same plaintext
- [ ] Test key rotation scenario
- [ ] Verify credentials work after encryption

**References:**
- https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes
- https://learn.microsoft.com/en-us/azure/key-vault/

---

## 📋 Other TODOs

### API Enhancements
- [ ] Add pagination to list endpoints
- [ ] Add filtering and sorting
- [ ] Implement rate limiting
- [ ] Add request validation with FluentValidation

### Real-time Features
- [ ] Implement SignalR hub for live telemetry
- [ ] Broadcast readings to connected clients
- [ ] Push alerts in real-time

### Monitoring & Observability
- [ ] Add Application Insights or similar
- [ ] Implement distributed tracing
- [ ] Add performance metrics
- [ ] Set up alerting for errors

### DevOps
- [ ] Set up CI/CD pipeline
- [ ] Add Docker Compose for full stack
- [ ] Create deployment scripts
- [ ] Set up staging environment

### Documentation
- [ ] API usage examples
- [ ] Architecture diagrams
- [ ] Deployment guide
- [ ] Troubleshooting guide

---

**Last Updated:** 2025-11-19
