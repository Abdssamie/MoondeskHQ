using Microsoft.Extensions.Configuration;
using Moq;
using Moondesk.BackgroundServices.Services;
using Xunit;

namespace Moondesk.BackgroundServices.Tests;

public class EncryptionServiceTests
{
    private readonly Mock<IConfiguration> _config;

    public EncryptionServiceTests()
    {
        _config = new Mock<IConfiguration>();
        // Key must be exactly 32 bytes or handled by padding logic in service. 
        // Service logic pads/cuts to 32 bytes, so we can test with a shorter or longer key too.
        _config.Setup(c => c["Security:EncryptionKey"]).Returns("TEST_KEY");
    }

    [Fact]
    public void Encrypt_Decrypt_RoundTrip_Success()
    {
        // Arrange
        var service = new EncryptionService(_config.Object);
        var originalText = "SuperSecretPassword123!";

        // Act
        var (encrypted, iv) = service.Encrypt(originalText);
        var decrypted = service.Decrypt(encrypted, iv);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(originalText, encrypted);
        
        Assert.NotNull(iv);
        Assert.NotEmpty(iv);
        
        Assert.Equal(originalText, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertext_ForSameInput_WithDifferentIV()
    {
        // Arrange
        var service = new EncryptionService(_config.Object);
        var text = "Data";

        // Act
        var (enc1, iv1) = service.Encrypt(text);
        var (enc2, iv2) = service.Encrypt(text);

        // Assert
        // AES with random IV should produce different ciphertexts for same plaintext
        // Note: EncryptionService generates new IV every time.
        Assert.NotEqual(iv1, iv2);
        Assert.NotEqual(enc1, enc2);
    }

    [Fact]
    public void Encrypt_EmptyString_ReturnsEmpty()
    {
        // Arrange
        var service = new EncryptionService(_config.Object);

        // Act
        var (encrypted, iv) = service.Encrypt("");

        // Assert
        Assert.Equal("", encrypted);
        Assert.Equal("", iv);
    }

    [Fact]
    public void Decrypt_EmptyString_ReturnsEmpty()
    {
        // Arrange
        var service = new EncryptionService(_config.Object);

        // Act
        var result = service.Decrypt("", "");

        // Assert
        Assert.Equal("", result);
    }
}
