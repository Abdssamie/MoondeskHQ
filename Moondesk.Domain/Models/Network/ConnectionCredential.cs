using System.ComponentModel.DataAnnotations;
using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models.Network;

public class ConnectionCredential
{
    public long Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    public required string EndpointUri { get; set; }

    public required string OrganizationId { get; set; }

    public string Username { get; set; } = string.Empty;

    [Required]
    public required string EncryptedPassword { get; set; }

    public required string EncryptionIV { get; set; }

    public Protocol Protocol { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2 || Name.Length > 100)
            throw new ArgumentException("Connection name must be between 2 and 100 characters.");
    }

    public void ValidateEndpoint()
    {
        if (string.IsNullOrWhiteSpace(EndpointUri) || !Uri.TryCreate(EndpointUri, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid endpoint URI format.");
    }
}