using System.ComponentModel.DataAnnotations;
using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models;

public class Organization
{
    public required string Id { get; set; }
    
    [MaxLength(100)]
    public required string Name { get; set; }
    
    public required string OwnerId { get; set; }
    
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;
    public int StorageLimitGB { get; set; } = 5;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrganizationMembership> Memberships { get; set; } = new List<OrganizationMembership>();

    public void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2 || Name.Length > 100)
            throw new ArgumentException("Organization name must be between 2 and 100 characters.");
    }

    public bool IsOwner(string userId) => OwnerId == userId;
}