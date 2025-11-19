using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Moondesk.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected string? UserId => HttpContext.Items["UserId"]?.ToString();
    protected string? OrganizationId => HttpContext.Items["OrganizationId"]?.ToString();

    protected bool IsAuthenticated() => !string.IsNullOrEmpty(UserId);
    protected bool HasOrganization() => !string.IsNullOrEmpty(OrganizationId);
}
