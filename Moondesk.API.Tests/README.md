# Moondesk.API.Tests

Comprehensive functional tests for API controllers following lessons learned from DataAccess testing.

## Test Coverage (28 tests)

### AssetsController (7 tests)
- ✅ GetAll returns assets when organization exists
- ✅ GetAll returns unauthorized when no organization
- ✅ GetById returns asset when exists
- ✅ GetById returns not found when doesn't exist
- ✅ Create returns created asset with organization ID
- ✅ Update returns no content when successful
- ✅ Delete returns no content when successful

### SensorsController (3 tests)
- ✅ GetAll returns all sensors without filter
- ✅ GetAll returns filtered sensors by asset ID
- ✅ Create sets sensor organization ID

### AlertsController (5 tests)
- ✅ GetAll returns all alerts without filter
- ✅ GetAll returns filtered alerts by acknowledged status
- ✅ GetBySensor returns alerts for specific sensor
- ✅ Acknowledge updates alert with user info
- ✅ Acknowledge returns not found for non-existent alert

### ReadingsController (4 tests)
- ✅ GetReadings returns recent readings with default limit
- ✅ GetReadings returns recent readings with custom limit
- ✅ GetBySensor returns all readings for sensor
- ✅ GetReadings returns unauthorized when no organization

### CommandsController (3 tests)
- ✅ GetPending returns pending commands
- ✅ GetBySensor returns commands for sensor
- ✅ Create sets organization and user ID

### UsersController (3 tests)
- ✅ GetCurrentUser returns user when authenticated
- ✅ GetCurrentUser returns not found when user doesn't exist
- ✅ GetOrganizationUsers returns users in organization

### OrganizationsController (3 tests)
- ✅ GetCurrent returns organization when exists
- ✅ GetCurrent returns not found when doesn't exist
- ✅ GetCurrent returns unauthorized when no organization

## Testing Approach

### Unit Testing with Moq
- Mock repository interfaces
- Test controller logic in isolation
- Verify authorization checks
- Validate response types and status codes

### Key Patterns

**1. HttpContext Setup**
```csharp
var httpContext = new DefaultHttpContext();
httpContext.Items["UserId"] = "user_test123";
httpContext.Items["OrganizationId"] = "org_test123";
controller.ControllerContext = new ControllerContext
{
    HttpContext = httpContext
};
```

**2. Repository Mocking**
```csharp
_mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(asset);
_mockRepo.Verify(r => r.AddAsync(It.IsAny<Asset>()), Times.Once);
```

**3. Authorization Testing**
```csharp
// Test unauthorized access
controller.HttpContext.Items["OrganizationId"] = null;
var result = await controller.GetAll();
Assert.IsType<UnauthorizedResult>(result);
```

## Running Tests

```bash
# Run all API tests
dotnet test Moondesk.API.Tests

# Run with detailed output
dotnet test Moondesk.API.Tests --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~AssetsControllerTests"
```

## Lessons Applied

1. **Minimal Mocking** - Only mock what's necessary (repositories)
2. **Clear Arrange-Act-Assert** - Each test follows AAA pattern
3. **Test Isolation** - Each test is independent
4. **Realistic Scenarios** - Tests cover actual use cases
5. **Authorization First** - Always test auth before business logic

## Future Enhancements

- [ ] Add integration tests with TestServer
- [ ] Test Clerk authentication middleware
- [ ] Add tests for remaining controllers (Users, Organizations, Commands)
- [ ] Test error handling and validation
- [ ] Add performance tests for high-volume scenarios
