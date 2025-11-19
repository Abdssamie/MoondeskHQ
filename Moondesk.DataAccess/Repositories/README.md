# Repository Implementation Guide

## Overview

Production-ready repository implementations following modern software design standards with comprehensive error handling, logging, and performance optimizations.

## Architecture Principles

### 1. **Separation of Concerns**
- Repositories handle only data access logic
- Business logic remains in domain services
- Clear interface contracts define behavior

### 2. **Error Handling Strategy**
```csharp
try
{
    // Repository operation
    _logger.LogDebug("Operation details");
    return await _context.SomeOperation();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error context with parameters");
    throw; // Re-throw to preserve stack trace
}
```

### 3. **Performance Optimizations**
- `AsNoTracking()` for read-only queries
- Bulk operations for high-throughput scenarios
- Proper indexing utilization
- TimescaleDB optimizations for time-series data

### 4. **Multi-Tenant Security**
- All IoT operations scoped by `organization_id`
- Validation of organization access
- Proper data isolation

## Repository Features

### **UserRepository**
- Comprehensive validation using domain methods
- Duplicate checking for email/username
- Optimistic concurrency control
- Soft delete support (if needed)

### **OrganizationRepository**
- Owner-based access control
- Name uniqueness validation
- Membership relationship management

### **ConnectionCredentialRepository**
- Encrypted credential storage
- Protocol-based filtering
- Active/inactive state management
- Last-used tracking for analytics

### **ReadingRepository**
- High-performance time-series operations
- TimescaleDB continuous aggregate support
- Bulk insert optimizations
- Statistical aggregation methods

## Usage Examples

### **Basic CRUD Operations**
```csharp
// Dependency injection
services.AddRepositories();

// Usage in service
public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Id = request.Id,
            Email = request.Email,
            Username = request.Username
            // ... other properties
        };
        
        return await _userRepository.CreateAsync(user);
    }
}
```

### **Time-Series Data Operations**
```csharp
// High-performance reading insertion
var readings = sensors.Select(s => new Reading
{
    OrganizationId = organizationId,
    SensorId = s.Id,
    Value = s.CurrentValue,
    Timestamp = DateTime.UtcNow
});

await _readingRepository.BulkInsertReadingsAsync(readings);

// Dashboard analytics using continuous aggregates
var hourlyData = await _readingRepository.GetHourlyAggregatesAsync(
    organizationId, sensorId, DateTime.Today.AddDays(-7), DateTime.Now);
```

### **Multi-Tenant Queries**
```csharp
// All queries automatically scoped by organization
var credentials = await _credentialRepository.GetByOrganizationIdAsync(organizationId);
var activeCredentials = await _credentialRepository.GetActiveAsync(organizationId);
```

## Error Handling

### **Domain Exceptions**
- `UserNotFoundException`
- `OrganizationNotFoundException`
- `ConnectionCredentialNotFoundException`
- `DuplicateEmailException`
- `DuplicateUsernameException`
- `DuplicateConnectionNameException`

### **Validation**
- Input parameter validation
- Domain model validation using built-in methods
- Business rule enforcement

## Logging Strategy

### **Log Levels**
- **Debug**: Query details, parameter values
- **Information**: Successful operations, creation/updates
- **Warning**: Deletion operations, state changes
- **Error**: Exceptions with full context

### **Structured Logging**
```csharp
_logger.LogInformation("Creating user: {UserId} ({Email})", user.Id, user.Email);
_logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
```

## Performance Considerations

### **Query Optimization**
- Use `AsNoTracking()` for read-only operations
- Include related data only when needed
- Leverage database indexes effectively

### **Bulk Operations**
- `AddRangeAsync()` for multiple inserts
- Batch size considerations for memory usage
- Transaction scope management

### **TimescaleDB Features**
- Continuous aggregates for dashboard queries
- Compression for historical data
- Partition pruning for time-range queries

## Testing Strategy

### **Unit Tests**
- Mock DbContext using in-memory provider
- Test validation logic
- Verify exception handling

### **Integration Tests**
- Test against real database
- Verify TimescaleDB features
- Performance benchmarking

## Monitoring and Observability

### **Metrics to Track**
- Query execution times
- Bulk operation throughput
- Error rates by repository
- Cache hit rates (if implemented)

### **Health Checks**
- Database connectivity
- Repository availability
- Performance thresholds

This implementation provides a solid foundation for scalable, maintainable, and performant data access in a multi-tenant IoT application.
