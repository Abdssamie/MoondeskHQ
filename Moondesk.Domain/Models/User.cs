using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Moondesk.Domain.Models;

public class User
{
    public required string Id { get; set; }
    
    [MaxLength(50)]
    public required string Username { get; set; }
    
    [MaxLength(50)]
    public required string Email { get; set; }
    
    [MaxLength(50)]
    public required string FirstName { get; set; }
    
    [MaxLength(50)]
    public required string LastName { get; set; }
    
    public bool IsOnboarded { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    public ICollection<OrganizationMembership> Memberships { get; set; } = new List<OrganizationMembership>();

    public void ValidateUsername()
    {
        if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 || Username.Length > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters.");
        
        if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9_-]+$"))
            throw new ArgumentException("Username can only contain letters, numbers, underscores, and hyphens.");
    }

    public void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format.");
    }
}