using Dapper;
using Microsoft.Data.SqlClient;
using DataAccessExamples.Models;

namespace DataAccessExamples.DapperRepositories;

/// <summary>
/// Order repository implementation using Dapper for high performance
/// </summary>
public class DapperOrderRepository
{
    private readonly string _connectionString;

    public DapperOrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Order?> GetByIdAsync(int orderId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT OrderId, UserId, OrderDate, TotalAmount, Status, ShippedDate
            FROM Orders
            WHERE OrderId = @OrderId";
        
        return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { OrderId = orderId });
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT OrderId, UserId, OrderDate, TotalAmount, Status, ShippedDate
            FROM Orders
            WHERE UserId = @UserId
            ORDER BY OrderDate DESC";
        
        return await connection.QueryAsync<Order>(sql, new { UserId = userId });
    }

    public async Task<Order?> GetWithItemsAsync(int orderId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT o.OrderId, o.UserId, o.OrderDate, o.TotalAmount, o.Status, o.ShippedDate,
                   u.UserId, u.Username, u.Email,
                   oi.OrderItemId, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                   p.ProductId, p.ProductName, p.Price
            FROM Orders o
            INNER JOIN Users u ON o.UserId = u.UserId
            LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.ProductId
            WHERE o.OrderId = @OrderId";
        
        Order? order = null;
        
        await connection.QueryAsync<Order, User, OrderItem, Product, Order>(
            sql,
            (o, u, oi, p) =>
            {
                if (order == null)
                {
                    order = o;
                    order.User = u;
                    order.OrderItems = new List<OrderItem>();
                }
                
                if (oi != null)
                {
                    oi.Product = p;
                    order.OrderItems.Add(oi);
                }
                
                return order;
            },
            new { OrderId = orderId },
            splitOn: "UserId,OrderItemId,ProductId");
        
        return order;
    }

    public async Task<int> CreateAsync(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            INSERT INTO Orders (UserId, OrderDate, TotalAmount, Status)
            VALUES (@UserId, @OrderDate, @TotalAmount, @Status);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        
        return await connection.ExecuteScalarAsync<int>(sql, order);
    }

    public async Task<int> UpdateAsync(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            UPDATE Orders
            SET TotalAmount = @TotalAmount,
                Status = @Status,
                ShippedDate = @ShippedDate
            WHERE OrderId = @OrderId";
        
        return await connection.ExecuteAsync(sql, order);
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT OrderId, UserId, OrderDate, TotalAmount, Status, ShippedDate
            FROM Orders
            WHERE Status = 'Pending'
            ORDER BY OrderDate";
        
        return await connection.QueryAsync<Order>(sql);
    }
}
