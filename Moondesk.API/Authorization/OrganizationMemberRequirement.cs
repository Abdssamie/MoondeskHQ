using Microsoft.AspNetCore.Authorization;

namespace Moondesk.API.Authorization;

public class OrganizationMemberRequirement : IAuthorizationRequirement
{
}

public class OrganizationAdminRequirement : IAuthorizationRequirement
{
}
