using DataAccessExamples.Models;
using DataAccessExamples.Repositories;

namespace DataAccessExamples.Services;

/// <summary>
/// User service that uses the repository pattern
/// </summary>
public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetActiveUsersAsync();
    }

    public async Task<User> CreateUserAsync(string username, string email)
    {
        // Business logic: Validate user doesn't already exist
        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with username '{username}' already exists");
        }

        var existingEmail = await _userRepository.GetByEmailAsync(email);
        if (existingEmail != null)
        {
            throw new InvalidOperationException($"User with email '{email}' already exists");
        }

        // Create user
        var user = new User
        {
            Username = username,
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user);
    }

    public async Task<User> UpdateUserAsync(int userId, string? email = null, bool? isActive = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Update properties if provided
        if (email != null)
        {
            user.Email = email;
        }

        if (isActive.HasValue)
        {
            user.IsActive = isActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return user;
    }

    public async Task DeactivateUserAsync(int userId)
    {
        await UpdateUserAsync(userId, isActive: false);
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<User>();
        }

        return await _userRepository.SearchUsersAsync(searchTerm);
    }
}
