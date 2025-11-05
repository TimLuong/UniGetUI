using Microsoft.EntityFrameworkCore;
using DataAccessExamples.Data;
using DataAccessExamples.Models;

namespace DataAccessExamples.Repositories;

/// <summary>
/// User-specific repository implementation
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<List<User>> GetUsersWithOrdersAsync()
    {
        return await _dbSet
            .Include(u => u.Orders)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.Username.Contains(searchTerm) || u.Email.Contains(searchTerm))
            .OrderBy(u => u.Username)
            .ToListAsync();
    }
}
