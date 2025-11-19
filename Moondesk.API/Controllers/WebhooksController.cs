using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Moondesk.API.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IConfiguration _configuration;

    public WebhooksController(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _configuration = configuration;
    }

    [HttpPost("clerk")]
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

        switch (webhook.type)
        {
            case "user.created":
                await HandleUserCreated(webhook.data);
                break;
            case "user.updated":
                await HandleUserUpdated(webhook.data);
                break;
            case "organization.created":
                await HandleOrganizationCreated(webhook.data);
                break;
            case "organization.updated":
                await HandleOrganizationUpdated(webhook.data);
                break;
        }

        return Ok();
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

        if (data.TryGetProperty("first_name", out var fn) && fn.GetString() is string first)
            user.FirstName = first;
        if (data.TryGetProperty("last_name", out var ln) && ln.GetString() is string last)
            user.LastName = last;

        await _userRepository.UpdateAsync(user);
    }

    private async Task HandleOrganizationCreated(JsonElement data)
    {
        var orgId = data.GetProperty("id").GetString()!;
        var name = data.GetProperty("name").GetString()!;
        var createdBy = data.GetProperty("created_by").GetString()!;

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

        if (data.TryGetProperty("name", out var name) && name.GetString() is string orgName)
            org.Name = orgName;

        await _organizationRepository.UpdateAsync(org);
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
        public string type { get; set; } = "";
        public JsonElement data { get; set; }
    }
}
