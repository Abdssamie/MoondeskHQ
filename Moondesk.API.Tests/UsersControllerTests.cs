using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moondesk.API.Controllers;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;

namespace Moondesk.API.Tests;

public class UsersControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IOrganizationMembershipRepository> _mockMembershipRepo;
    private readonly UsersController _controller;
    private const string TestOrgId = "org_test123";
    private const string TestUserId = "user_test123";

    public UsersControllerTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockMembershipRepo = new Mock<IOrganizationMembershipRepository>();
        _controller = new UsersController(_mockUserRepo.Object, _mockMembershipRepo.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = TestUserId;
        httpContext.Items["OrganizationId"] = TestOrgId;
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUser_WhenAuthenticated()
    {
        // Arrange
        var user = new User 
        { 
            Id = TestUserId, 
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(TestUserId)).ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(TestUserId, returnedUser.Id);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(TestUserId)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetOrganizationUsers_ReturnsUsersInOrganization()
    {
        // Arrange
        var memberships = new List<OrganizationMembership>
        {
            new() { UserId = "user1", OrganizationId = TestOrgId },
            new() { UserId = "user2", OrganizationId = TestOrgId }
        };
        _mockMembershipRepo.Setup(r => r.GetByOrganizationIdAsync(TestOrgId)).ReturnsAsync(memberships);
        
        _mockUserRepo.Setup(r => r.GetByIdAsync("user1")).ReturnsAsync(
            new User { Id = "user1", Username = "user1", Email = "user1@test.com", FirstName = "User", LastName = "One" });
        _mockUserRepo.Setup(r => r.GetByIdAsync("user2")).ReturnsAsync(
            new User { Id = "user2", Username = "user2", Email = "user2@test.com", FirstName = "User", LastName = "Two" });

        // Act
        var result = await _controller.GetOrganizationUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Equal(2, users.Count());
    }
}
