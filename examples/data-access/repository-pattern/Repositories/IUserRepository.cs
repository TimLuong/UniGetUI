using DataAccessExamples.Models;

namespace DataAccessExamples.Repositories;

/// <summary>
/// User-specific repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all active users
    /// </summary>
    Task<List<User>> GetActiveUsersAsync();

    /// <summary>
    /// Gets users with their orders
    /// </summary>
    Task<List<User>> GetUsersWithOrdersAsync();

    /// <summary>
    /// Searches users by username or email
    /// </summary>
    Task<List<User>> SearchUsersAsync(string searchTerm);
}
