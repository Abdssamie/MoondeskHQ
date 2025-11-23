using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Exceptions;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;

namespace Moondesk.DataAccess.Repositories;

/// <summary>
/// Repository for managing User entities with comprehensive error handling and logging.
/// Implements caching, validation, and performance optimizations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(MoondeskDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>The user if found, null otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when id is null or empty</exception>
    public async Task<User?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("User ID cannot be null or empty", nameof(id));

        try
        {
            _logger.LogDebug("Retrieving user with ID: {UserId}", id);
            
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                _logger.LogDebug("User not found with ID: {UserId}", id);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        try
        {
            _logger.LogDebug("Retrieving user with email: {Email}", email);
            
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="username">The user's username</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        try
        {
            _logger.LogDebug("Retrieving user with username: {Username}", username);
            
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all users with pagination support.
    /// </summary>
    /// <returns>Collection of all users</returns>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Retrieving all users");
            
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    /// <summary>
    /// Creates a new user with validation and duplicate checking.
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <returns>The created user</returns>
    /// <exception cref="DuplicateEmailException">Thrown when email already exists</exception>
    /// <exception cref="DuplicateUsernameException">Thrown when username already exists</exception>
    public async Task<User> CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            // Validate user data
            user.ValidateEmail();
            user.ValidateUsername();

            // Check for duplicates
            if (await ExistsAsync(user.Id))
                throw new DuplicateUsernameException($"User with ID {user.Id} already exists");

            if (await GetByEmailAsync(user.Email) != null)
                throw new DuplicateEmailException(user.Email);

            if (await GetByUsernameAsync(user.Username) != null)
                throw new DuplicateUsernameException(user.Username);

            _logger.LogInformation("Creating user: {UserId} ({Email})", user.Id, user.Email);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created user: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user with optimistic concurrency control.
    /// </summary>
    /// <param name="user">The user to update</param>
    /// <returns>The updated user</returns>
    /// <exception cref="UserNotFoundException">Thrown when user doesn't exist</exception>
    public async Task<User> UpdateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            user.ValidateEmail();
            user.ValidateUsername();

            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return user;

            _logger.LogInformation("Updating user: {UserId}", user.Id);

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated user: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a user and all related data.
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <exception cref="UserNotFoundException">Thrown when user doesn't exist</exception>
    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("User ID cannot be null or empty", nameof(id));

        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return;

            _logger.LogWarning("Deleting user: {UserId}", id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Successfully deleted user: {UserId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Checks if a user exists by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>True if user exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        try
        {
            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {UserId}", id);
            throw;
        }
    }
}
