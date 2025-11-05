using DataAccessExamples.Models;

namespace DataAccessExamples.Repositories;

/// <summary>
/// Order-specific repository interface
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Gets orders for a specific user
    /// </summary>
    Task<List<Order>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Gets an order with its items and product details
    /// </summary>
    Task<Order?> GetWithItemsAsync(int orderId);

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    Task<List<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets orders by status
    /// </summary>
    Task<List<Order>> GetByStatusAsync(string status);

    /// <summary>
    /// Gets pending orders
    /// </summary>
    Task<List<Order>> GetPendingOrdersAsync();
}
