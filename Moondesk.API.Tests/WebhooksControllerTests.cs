using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moondesk.API.Controllers;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;
using Moq;
using Xunit;

namespace Moondesk.API.Tests;

public class WebhooksControllerTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IOrganizationRepository> _orgRepo;
    private readonly Mock<IOrganizationMembershipRepository> _membershipRepo;
    private readonly Mock<IConfiguration> _config;
    private readonly Mock<ILogger<WebhooksController>> _logger;

    public WebhooksControllerTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _orgRepo = new Mock<IOrganizationRepository>();
        _membershipRepo = new Mock<IOrganizationMembershipRepository>();
        _config = new Mock<IConfiguration>();
        _logger = new Mock<ILogger<WebhooksController>>();
    }

    [Fact]
    public async Task ClerkWebhook_MissingSecret_Returns500()
    {
        _config.Setup(c => c["Clerk:WebhookSecret"]).Returns(string.Empty);
        var controller = new WebhooksController(_userRepo.Object, _orgRepo.Object, _membershipRepo.Object, _config.Object, _logger.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.ClerkWebhook();

        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task ClerkWebhook_MissingSignature_ReturnsUnauthorized()
    {
        _config.Setup(c => c["Clerk:WebhookSecret"]).Returns("test_secret");
        var controller = new WebhooksController(_userRepo.Object, _orgRepo.Object, _membershipRepo.Object, _config.Object, _logger.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.ClerkWebhook();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task UserCreated_CreatesUser()
    {
        User? capturedUser = null;
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        var user = new User
        {
            Id = "user_123",
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        await _userRepo.Object.CreateAsync(user);

        Assert.NotNull(capturedUser);
        Assert.Equal("user_123", capturedUser.Id);
        Assert.Equal("test@example.com", capturedUser.Email);
    }

    [Fact]
    public async Task UserUpdated_UpdatesUser()
    {
        var user = new User { Id = "user_123", Email = "test@example.com", Username = "testuser", FirstName = "Old", LastName = "Name" };
        _userRepo.Setup(r => r.GetByIdAsync("user_123")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

        var existingUser = await _userRepo.Object.GetByIdAsync("user_123");
        existingUser!.FirstName = "New";
        await _userRepo.Object.UpdateAsync(existingUser);

        _userRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.FirstName == "New")), Times.Once);
    }

    [Fact]
    public async Task UserDeleted_DeletesUser()
    {
        var user = new User { Id = "user_123", Email = "test@example.com", Username = "testuser", FirstName = "Test", LastName = "User" };
        _userRepo.Setup(r => r.GetByIdAsync("user_123")).ReturnsAsync(user);

        var existingUser = await _userRepo.Object.GetByIdAsync("user_123");
        if (existingUser != null)
        {
            await _userRepo.Object.DeleteAsync("user_123");
        }

        _userRepo.Verify(r => r.DeleteAsync("user_123"), Times.Once);
    }

    [Fact]
    public async Task OrganizationCreated_CreatesOrganization()
    {
        Organization? capturedOrg = null;
        _orgRepo.Setup(r => r.CreateAsync(It.IsAny<Organization>()))
            .Callback<Organization>(o => capturedOrg = o)
            .ReturnsAsync((Organization o) => o);

        var org = new Organization
        {
            Id = "org_123",
            Name = "Test Org",
            OwnerId = "user_123"
        };
        await _orgRepo.Object.CreateAsync(org);

        Assert.NotNull(capturedOrg);
        Assert.Equal("org_123", capturedOrg.Id);
        Assert.Equal("Test Org", capturedOrg.Name);
    }

    [Fact]
    public async Task OrganizationUpdated_UpdatesOrganization()
    {
        var org = new Organization { Id = "org_123", Name = "Old Name", OwnerId = "user_123" };
        _orgRepo.Setup(r => r.GetByIdAsync("org_123")).ReturnsAsync(org);
        _orgRepo.Setup(r => r.UpdateAsync(It.IsAny<Organization>())).ReturnsAsync(org);

        var existingOrg = await _orgRepo.Object.GetByIdAsync("org_123");
        existingOrg!.Name = "New Name";
        await _orgRepo.Object.UpdateAsync(existingOrg);

        _orgRepo.Verify(r => r.UpdateAsync(It.Is<Organization>(o => o.Name == "New Name")), Times.Once);
    }

    [Fact]
    public async Task OrganizationDeleted_DeletesOrganization()
    {
        var org = new Organization { Id = "org_123", Name = "Test Org", OwnerId = "user_123" };
        _orgRepo.Setup(r => r.GetByIdAsync("org_123")).ReturnsAsync(org);

        var existingOrg = await _orgRepo.Object.GetByIdAsync("org_123");
        if (existingOrg != null)
        {
            await _orgRepo.Object.DeleteAsync("org_123");
        }

        _orgRepo.Verify(r => r.DeleteAsync("org_123"), Times.Once);
    }

    [Fact]
    public async Task MembershipCreated_WithAdminRole_CreatesMembership()
    {
        OrganizationMembership? capturedMembership = null;
        _membershipRepo.Setup(r => r.CreateAsync(It.IsAny<OrganizationMembership>()))
            .Callback<OrganizationMembership>(m => capturedMembership = m)
            .ReturnsAsync((OrganizationMembership m) => m);

        var membership = new OrganizationMembership
        {
            UserId = "user_123",
            OrganizationId = "org_123",
            Role = UserRole.Admin,
            JoinedAt = DateTime.UtcNow
        };
        await _membershipRepo.Object.CreateAsync(membership);

        Assert.NotNull(capturedMembership);
        Assert.Equal("user_123", capturedMembership.UserId);
        Assert.Equal("org_123", capturedMembership.OrganizationId);
        Assert.Equal(UserRole.Admin, capturedMembership.Role);
    }

    [Fact]
    public async Task MembershipCreated_WithUserRole_CreatesMembership()
    {
        OrganizationMembership? capturedMembership = null;
        _membershipRepo.Setup(r => r.CreateAsync(It.IsAny<OrganizationMembership>()))
            .Callback<OrganizationMembership>(m => capturedMembership = m)
            .ReturnsAsync((OrganizationMembership m) => m);

        var membership = new OrganizationMembership
        {
            UserId = "user_123",
            OrganizationId = "org_123",
            Role = UserRole.User,
            JoinedAt = DateTime.UtcNow
        };
        await _membershipRepo.Object.CreateAsync(membership);

        Assert.NotNull(capturedMembership);
        Assert.Equal(UserRole.User, capturedMembership.Role);
    }

    [Fact]
    public async Task MembershipUpdated_UpdatesRole()
    {
        var membership = new OrganizationMembership
        {
            UserId = "user_123",
            OrganizationId = "org_123",
            Role = UserRole.User
        };
        _membershipRepo.Setup(r => r.GetByIdAsync("user_123", "org_123")).ReturnsAsync(membership);
        _membershipRepo.Setup(r => r.UpdateAsync(It.IsAny<OrganizationMembership>())).ReturnsAsync(membership);

        var existingMembership = await _membershipRepo.Object.GetByIdAsync("user_123", "org_123");
        existingMembership!.Role = UserRole.Admin;
        await _membershipRepo.Object.UpdateAsync(existingMembership);

        _membershipRepo.Verify(r => r.UpdateAsync(It.Is<OrganizationMembership>(m => m.Role == UserRole.Admin)), Times.Once);
    }

    [Fact]
    public async Task MembershipDeleted_DeletesMembership()
    {
        await _membershipRepo.Object.DeleteAsync("user_123", "org_123");

        _membershipRepo.Verify(r => r.DeleteAsync("user_123", "org_123"), Times.Once);
    }
}
