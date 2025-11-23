using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;

namespace Moondesk.API.Authorization;

public class OrganizationMemberHandler : AuthorizationHandler<OrganizationMemberRequirement>
{
    private readonly IOrganizationMembershipRepository _membershipRepo;
    private readonly ILogger<OrganizationMemberHandler> _logger;

    public OrganizationMemberHandler(
        IOrganizationMembershipRepository membershipRepo,
        ILogger<OrganizationMemberHandler> logger)
    {
        _membershipRepo = membershipRepo;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationMemberRequirement requirement)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        var orgId = context.User.FindFirst("org_id")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(orgId))
        {
            _logger.LogWarning("Missing user ID or organization ID in claims");
            return;
        }

        var membership = await _membershipRepo.GetByIdAsync(userId, orgId);
        if (membership != null)
        {
            context.Succeed(requirement);
        }
    }
}

public class OrganizationAdminHandler : AuthorizationHandler<OrganizationAdminRequirement>
{
    private readonly IOrganizationMembershipRepository _membershipRepo;
    private readonly ILogger<OrganizationAdminHandler> _logger;

    public OrganizationAdminHandler(
        IOrganizationMembershipRepository membershipRepo,
        ILogger<OrganizationAdminHandler> logger)
    {
        _membershipRepo = membershipRepo;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationAdminRequirement requirement)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        var orgId = context.User.FindFirst("org_id")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(orgId))
        {
            _logger.LogWarning("Missing user ID or organization ID in claims");
            return;
        }

        var membership = await _membershipRepo.GetByIdAsync(userId, orgId);
        if (membership?.Role == UserRole.Admin)
        {
            context.Succeed(requirement);
        }
    }
}
