using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moondesk.Domain.Interfaces.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace Moondesk.API.Controllers;

[SwaggerTag("Organization information")]
[Authorize(Policy = "OrgAdmin")]
public class OrganizationsController : BaseApiController
{
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationsController(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    [HttpGet("current")]
    [SwaggerOperation(Summary = "Get current organization", Description = "Get details of the user's current organization")]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "Organization not found")]
    public async Task<IActionResult> GetCurrent()
    {
        if (!HasOrganization()) return Unauthorized();

        var org = await _organizationRepository.GetByIdAsync(OrganizationId!);
        if (org == null) return NotFound();

        return Ok(org);
    }
}
