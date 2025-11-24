using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Interfaces.Services;
using Moondesk.Domain.Models.Network;

namespace Moondesk.API.Controllers;

[ApiController]
[Route("api/v1/connection-credentials")]
[Authorize] // Requires valid user
public class ConnectionCredentialsController : BaseApiController
{
    private readonly IConnectionCredentialRepository _repository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ConnectionCredentialsController> _logger;

    public ConnectionCredentialsController(
        IConnectionCredentialRepository repository,
        IEncryptionService encryptionService,
        ILogger<ConnectionCredentialsController> logger)
    {
        _repository = repository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "OrgMember")]
    public async Task<ActionResult<IEnumerable<ConnectionCredential>>> GetCredentials()
    {
        if (!HasOrganization()) return BadRequest("Organization ID missing");
        var orgId = OrganizationId!;

        var credentials = await _repository.GetByOrganizationIdAsync(orgId);
        // Do not return encrypted passwords/IVs or decrypt them for listing
        foreach (var cred in credentials)
        {
            cred.EncryptedPassword = "***";
            cred.EncryptionIV = "***";
        }
        return Ok(credentials);
    }

    [HttpPost]
    [Authorize(Policy = "OrgAdmin")]
    public async Task<ActionResult<ConnectionCredential>> CreateCredential(CreateCredentialRequest request)
    {
        if (!HasOrganization()) return BadRequest("Organization ID missing");
        var orgId = OrganizationId!;

        var (encryptedPassword, iv) = _encryptionService.Encrypt(request.Password);

        var credential = new ConnectionCredential
        {
            Name = request.Name,
            EndpointUri = request.EndpointUri,
            OrganizationId = orgId,
            Username = request.Username,
            ClientId = request.ClientId,
            Protocol = request.Protocol,
            EncryptedPassword = encryptedPassword,
            EncryptionIV = iv,
            IsActive = true
        };

        try
        {
            var created = await _repository.CreateAsync(credential);
            return CreatedAtAction(nameof(GetCredentials), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create credential");
            return BadRequest(ex.Message);
        }
    }

    public class CreateCredentialRequest
    {
        public required string Name { get; set; }
        public required string EndpointUri { get; set; }
        public string Username { get; set; } = string.Empty;
        public required string Password { get; set; }
        public string? ClientId { get; set; }
        public Protocol Protocol { get; set; }
    }
}
