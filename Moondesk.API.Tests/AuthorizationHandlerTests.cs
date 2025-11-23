using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using Moondesk.API.Authorization;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;
using Xunit;

namespace Moondesk.API.Tests;

public class AuthorizationHandlerTests
{
    private readonly Mock<IOrganizationMembershipRepository> _membershipRepo;
    private readonly Mock<ILogger<OrganizationMemberHandler>> _memberLogger;
    private readonly Mock<ILogger<OrganizationAdminHandler>> _adminLogger;

    public AuthorizationHandlerTests()
    {
        _membershipRepo = new Mock<IOrganizationMembershipRepository>();
        _memberLogger = new Mock<ILogger<OrganizationMemberHandler>>();
        _adminLogger = new Mock<ILogger<OrganizationAdminHandler>>();
    }

    [Fact]
    public async Task OrganizationMemberHandler_Succeeds_WhenUserIsMember()
    {
        // Arrange
        var handler = new OrganizationMemberHandler(_membershipRepo.Object, _memberLogger.Object);
        var requirement = new OrganizationMemberRequirement();
        
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123"),
            new Claim("org_id", "org_123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        var membership = new OrganizationMembership
        {
            UserId = "user_123",
            OrganizationId = "org_123",
            Role = UserRole.User
        };
        _membershipRepo.Setup(r => r.GetByIdAsync("user_123", "org_123")).ReturnsAsync(membership);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task OrganizationMemberHandler_Fails_WhenUserIsNotMember()
    {
        // Arrange
        var handler = new OrganizationMemberHandler(_membershipRepo.Object, _memberLogger.Object);
        var requirement = new OrganizationMemberRequirement();
        
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123"),
            new Claim("org_id", "org_123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        _membershipRepo.Setup(r => r.GetByIdAsync("user_123", "org_123")).ReturnsAsync((OrganizationMembership?)null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task OrganizationMemberHandler_Fails_WhenMissingClaims()
    {
        // Arrange
        var handler = new OrganizationMemberHandler(_membershipRepo.Object, _memberLogger.Object);
        var requirement = new OrganizationMemberRequirement();
        
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task OrganizationAdminHandler_Succeeds_WhenUserIsAdmin()
    {
        // Arrange
        var handler = new OrganizationAdminHandler(_membershipRepo.Object, _adminLogger.Object);
        var requirement = new OrganizationAdminRequirement();
        
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123"),
            new Claim("org_id", "org_123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        var membership = new OrganizationMembership
        {
            UserId = "user_123",
            OrganizationId = "org_123",
            Role = UserRole.Admin
        };
        _membershipRepo.Setup(r => r.GetByIdAsync("user_123", "org_123")).ReturnsAsync(membership);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task OrganizationAdminHandler_Fails_WhenUserIsNotAdmin()
    {
        // Arrange
        var handler = new OrganizationAdminHandler(_membershipRepo.Object, _adminLogger.Object);
        var requirement = new OrganizationAdminRequirement();
        
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123"),
            new Claim("org_id", "org_123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        var membership = new OrganizationMembership
        {
            UserId = "user_123",
            OrganizationId = "org_123",
            Role = UserRole.User
        };
        _membershipRepo.Setup(r => r.GetByIdAsync("user_123", "org_123")).ReturnsAsync(membership);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task OrganizationAdminHandler_Fails_WhenMembershipNotFound()
    {
        // Arrange
        var handler = new OrganizationAdminHandler(_membershipRepo.Object, _adminLogger.Object);
        var requirement = new OrganizationAdminRequirement();
        
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123"),
            new Claim("org_id", "org_123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        _membershipRepo.Setup(r => r.GetByIdAsync("user_123", "org_123")).ReturnsAsync((OrganizationMembership?)null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }
}
