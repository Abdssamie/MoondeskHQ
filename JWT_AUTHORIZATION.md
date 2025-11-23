# JWT Authorization Implementation (Option 2 - Enterprise Grade)

## What Was Implemented

### 1. **Custom Authorization Requirements**
Created two authorization requirements:
- `OrganizationMemberRequirement` - User must be a member of the organization
- `OrganizationAdminRequirement` - User must be an admin of the organization

### 2. **Authorization Handlers**
- `OrganizationMemberHandler` - Validates user is a member by checking database
- `OrganizationAdminHandler` - Validates user has Admin role in organization

### 3. **Enhanced Middleware**
Updated `ClerkAuthenticationMiddleware` to:
- **Reject unauthenticated requests** with 401 status
- **Skip authentication** for `/api/v1/webhooks` and `/health` endpoints
- **Set ClaimsPrincipal** on HttpContext for authorization pipeline
- **Extract claims** (`sub` for user ID, `org_id` for organization ID)

### 4. **Authorization Policies**
Registered two policies in `Program.cs`:
- `OrgMember` - Requires OrganizationMemberRequirement
- `OrgAdmin` - Requires OrganizationAdminRequirement

### 5. **Controller Protection**
Applied authorization attributes:

**Member-level access:**
- `AssetsController` - [Authorize(Policy = "OrgMember")]
- `SensorsController` - [Authorize(Policy = "OrgMember")]
- `ReadingsController` - [Authorize(Policy = "OrgMember")]
- `AlertsController` - [Authorize(Policy = "OrgMember")]
- `CommandsController` - [Authorize(Policy = "OrgMember")]

**Admin-level access:**
- `UsersController` - [Authorize(Policy = "OrgAdmin")]
- `OrganizationsController` - [Authorize(Policy = "OrgAdmin")]

**No authentication required:**
- `WebhooksController` - Uses signature verification instead
- `/health` endpoint - Public health check

## Security Flow

```
1. Client sends request with JWT token in Authorization header
   ↓
2. ClerkAuthenticationMiddleware validates JWT signature
   ↓
3. If valid, extracts claims (user_id, org_id) and sets ClaimsPrincipal
   ↓
4. Authorization middleware checks [Authorize] attribute
   ↓
5. Authorization handler queries database to verify membership/role
   ↓
6. If authorized, request proceeds to controller
   ↓
7. If unauthorized, returns 403 Forbidden
```

## What This Prevents

### 1. **Unauthenticated Access**
- All API endpoints (except webhooks/health) require valid JWT token
- Invalid/expired tokens get 401 Unauthorized

### 2. **Cross-Organization Access**
- Users can only access data from their own organization
- Authorization handlers verify org membership in database

### 3. **Privilege Escalation**
- Regular users cannot access admin endpoints
- Role checked against database, not just JWT claims

### 4. **Token Tampering**
- Clerk validates JWT signature using RS256
- Claims cannot be modified without invalidating signature

## Testing Authorization

### Test Member Access
```bash
# Valid member token - should succeed
curl -H "Authorization: Bearer <member_token>" \
  https://api.moondesk.com/api/assets

# No token - should return 401
curl https://api.moondesk.com/api/assets

# Token from different org - should return 403
curl -H "Authorization: Bearer <other_org_token>" \
  https://api.moondesk.com/api/assets
```

### Test Admin Access
```bash
# Member token on admin endpoint - should return 403
curl -H "Authorization: Bearer <member_token>" \
  https://api.moondesk.com/api/users

# Admin token - should succeed
curl -H "Authorization: Bearer <admin_token>" \
  https://api.moondesk.com/api/users
```

## Additional Security Considerations

### 1. **Token Expiration**
- Clerk tokens expire after 1 hour by default
- Frontend should implement token refresh logic

### 2. **Token Revocation**
- Clerk handles token revocation when user is deleted
- Webhook handlers sync user/org deletions to database

### 3. **Audit Logging**
- All authorization failures are logged
- Monitor logs for suspicious activity

### 4. **Rate Limiting**
- Global rate limit prevents brute force attacks
- Even with valid tokens, requests are rate limited

## Performance Impact

- **Database query per request** to verify membership
- Consider caching membership data with short TTL (5-10 minutes)
- Authorization handlers are async and non-blocking

## Future Enhancements

1. **Caching** - Cache membership lookups in Redis
2. **Fine-grained permissions** - Add resource-level permissions
3. **API scopes** - Implement OAuth2 scopes for third-party apps
4. **Audit trail** - Log all data access for compliance
5. **IP whitelisting** - Restrict admin endpoints to specific IPs
