using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;
using Moondesk.Domain.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Moondesk.API.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
[EnableRateLimiting("webhook")]
public class WebhooksController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationMembershipRepository _membershipRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IOrganizationMembershipRepository membershipRepository,
        IConfiguration configuration,
        ILogger<WebhooksController> logger)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ClerkWebhook()
    {
        var webhookSecret = _configuration["Clerk:WebhookSecret"];
        if (string.IsNullOrEmpty(webhookSecret))
            return StatusCode(500, "Webhook secret not configured");

        var signature = Request.Headers["svix-signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
            return Unauthorized("Missing signature");

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        if (!VerifyWebhook(payload, signature, webhookSecret))
            return Unauthorized("Invalid signature");

        var webhook = JsonSerializer.Deserialize<ClerkWebhookEvent>(payload);
        if (webhook == null) return BadRequest();

        _logger.LogInformation("Received webhook with ID {Id} and event type {EventType}", webhook.Data.TryGetProperty("id", out var id) ? id.GetString() : "unknown", webhook.Type);

        switch (webhook.Type)
        {
            case "user.created":
                await HandleUserCreated(webhook.Data);
                break;
            case "user.updated":
                await HandleUserUpdated(webhook.Data);
                break;
            case "user.deleted":
                await HandleUserDeleted(webhook.Data);
                break;
            case "organization.created":
                await HandleOrganizationCreated(webhook.Data);
                break;
            case "organization.updated":
                await HandleOrganizationUpdated(webhook.Data);
                break;
            case "organization.deleted":
                await HandleOrganizationDeleted(webhook.Data);
                break;
            case "organizationMembership.created":
                await HandleMembershipCreated(webhook.Data);
                break;
            case "organizationMembership.updated":
                await HandleMembershipUpdated(webhook.Data);
                break;
            case "organizationMembership.deleted":
                await HandleMembershipDeleted(webhook.Data);
                break;
        }

        return Ok(new { message = "Webhook received" });
    }

    private async Task HandleUserCreated(JsonElement data)
    {
        var userId = data.GetProperty("id").GetString()!;
        var email = data.GetProperty("email_addresses")[0].GetProperty("email_address").GetString()!;
        var username = data.TryGetProperty("username", out var un) ? un.GetString() : email.Split('@')[0];
        var firstName = data.TryGetProperty("first_name", out var fn) ? fn.GetString() : "";
        var lastName = data.TryGetProperty("last_name", out var ln) ? ln.GetString() : "";

        var user = new User
        {
            Id = userId,
            Username = username ?? email.Split('@')[0],
            Email = email,
            FirstName = firstName ?? "",
            LastName = lastName ?? ""
        };

        await _userRepository.CreateAsync(user);
    }

    private async Task HandleUserUpdated(JsonElement data)
    {
        var userId = data.GetProperty("id").GetString()!;
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        if (data.TryGetProperty("first_name", out var fn) && fn.GetString() is { } first)
            user.FirstName = first;
        if (data.TryGetProperty("last_name", out var ln) && ln.GetString() is {} last)
            user.LastName = last;

        await _userRepository.UpdateAsync(user);
    }

    private async Task HandleOrganizationCreated(JsonElement data)
    {
        var orgId = data.GetProperty("id").GetString()!;
        var name = data.GetProperty("name").GetString()!;
        var createdBy = data.GetProperty("created_by").GetString()!;

        if (await _organizationRepository.ExistsAsync(orgId))
        {
            _logger.LogWarning("Organization {OrgId} already exists. Skipping creation.", orgId);
            return;
        }

        var org = new Organization
        {
            Id = orgId,
            Name = name,
            OwnerId = createdBy
        };

        await _organizationRepository.CreateAsync(org);
    }

    private async Task HandleOrganizationUpdated(JsonElement data)
    {
        var orgId = data.GetProperty("id").GetString()!;
        var org = await _organizationRepository.GetByIdAsync(orgId);
        if (org == null) return;

        if (data.TryGetProperty("name", out var name) && name.GetString() is { } orgName)
            org.Name = orgName;

        await _organizationRepository.UpdateAsync(org);
    }


    private async Task HandleUserDeleted(JsonElement data)
    {
        var userId = data.GetProperty("id").GetString()!;
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            await _userRepository.DeleteAsync(userId);
            _logger.LogInformation("Deleted user {UserId}", userId);
        }
    }

    private async Task HandleOrganizationDeleted(JsonElement data)
    {
        var orgId = data.GetProperty("id").GetString()!;
        var org = await _organizationRepository.GetByIdAsync(orgId);
        if (org != null)
        {
            await _organizationRepository.DeleteAsync(orgId);
            _logger.LogInformation("Deleted organization {OrgId}", orgId);
        }
    }

    private async Task HandleMembershipCreated(JsonElement data)
    {
        var orgId = data.GetProperty("organization").GetProperty("id").GetString()!;
        var userId = data.GetProperty("public_user_data").GetProperty("user_id").GetString()!;
        var role = data.GetProperty("role").GetString()!;

        var membership = new OrganizationMembership
        {
            UserId = userId,
            OrganizationId = orgId,
            Role = role == "admin" ? UserRole.Admin : UserRole.User,
            JoinedAt = DateTime.UtcNow
        };

        await _membershipRepository.CreateAsync(membership);
        _logger.LogInformation("Created membership for user {UserId} in org {OrgId} with role {Role}", userId, orgId, membership.Role);
    }

    private async Task HandleMembershipUpdated(JsonElement data)
    {
        var orgId = data.GetProperty("organization").GetProperty("id").GetString()!;
        var userId = data.GetProperty("public_user_data").GetProperty("user_id").GetString()!;
        var role = data.GetProperty("role").GetString()!;

        var membership = await _membershipRepository.GetByIdAsync(userId, orgId);
        if (membership != null)
        {
            membership.Role = role == "admin" ? UserRole.Admin : UserRole.User;
            await _membershipRepository.UpdateAsync(membership);
            _logger.LogInformation("Updated membership for user {UserId} in org {OrgId} to role {Role}", userId, orgId, membership.Role);
        }
    }

    private async Task HandleMembershipDeleted(JsonElement data)
    {
        var orgId = data.GetProperty("organization").GetProperty("id").GetString()!;
        var userId = data.GetProperty("public_user_data").GetProperty("user_id").GetString()!;

        await _membershipRepository.DeleteAsync(userId, orgId);
        _logger.LogInformation("Deleted membership for user {UserId} from org {OrgId}", userId, orgId);
    }

    private bool VerifyWebhook(string payload, string signature, string secret)
    {
        var parts = signature.Split(',');
        var timestamp = parts[0].Split('=')[1];
        var signatures = parts.Skip(1).Select(p => p.Split('=')[1]).ToArray();

        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload)));

        return signatures.Any(s => s == hash);
    }

    private class ClerkWebhookEvent
    {
        public string Type { get; set; } = "";
        public JsonElement Data { get; set; }
    }
}
