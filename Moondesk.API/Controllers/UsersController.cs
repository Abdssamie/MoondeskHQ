using Microsoft.AspNetCore.Mvc;
using Moondesk.Domain.Interfaces.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace Moondesk.API.Controllers;

[SwaggerTag("User management within organization scope")]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationMembershipRepository _membershipRepository;

    public UsersController(
        IUserRepository userRepository,
        IOrganizationMembershipRepository membershipRepository)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
    }

    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get current user", Description = "Get the authenticated user's profile")]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "User not found")]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (!IsAuthenticated()) return Unauthorized();

        var user = await _userRepository.GetByIdAsync(UserId!);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpGet("by_organization")]
    [SwaggerOperation(Summary = "List organization users", Description = "Get all users in the current organization")]
    [SwaggerResponse(200, "Success")]
    public async Task<IActionResult> GetOrganizationUsers()
    {
        if (!HasOrganization()) return Unauthorized();

        var memberships = await _membershipRepository.GetByOrganizationIdAsync(OrganizationId!);
        var userIds = memberships.Select(m => m.UserId).ToList();
        
        var users = new List<object>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                users.Add(new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName
                });
            }
        }

        return Ok(users);
    }
}
