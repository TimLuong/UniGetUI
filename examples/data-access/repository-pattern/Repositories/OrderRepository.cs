using Microsoft.EntityFrameworkCore;
using DataAccessExamples.Data;
using DataAccessExamples.Models;

namespace DataAccessExamples.Repositories;

/// <summary>
/// Order-specific repository implementation
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Order>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetWithItemsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        return await GetByStatusAsync("Pending");
    }
}
