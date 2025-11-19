using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models;

public class OrganizationMembership
{
    public required string UserId { get; set; }
    public required string OrganizationId { get; set; }

    public UserRole Role { get; set; } = UserRole.User;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Dictionary<string, string> Metadata { get; set; } = new();
    
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}