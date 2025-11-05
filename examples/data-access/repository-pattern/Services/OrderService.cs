using DataAccessExamples.Models;
using DataAccessExamples.UnitOfWork;

namespace DataAccessExamples.Services;

/// <summary>
/// Order service that uses the Unit of Work pattern for transactions
/// </summary>
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Order?> GetOrderAsync(int orderId)
    {
        return await _unitOfWork.Orders.GetByIdAsync(orderId);
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _unitOfWork.Orders.GetWithItemsAsync(orderId);
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await _unitOfWork.Orders.GetByUserIdAsync(userId);
    }

    public async Task<Order> CreateOrderAsync(int userId, List<(int productId, int quantity, decimal unitPrice)> items)
    {
        try
        {
            // Start transaction
            await _unitOfWork.BeginTransactionAsync();

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = items.Sum(i => i.quantity * i.unitPrice)
            };

            order = await _unitOfWork.Orders.AddAsync(order);

            // Note: In a real implementation, you would add OrderItems here
            // This example focuses on the transaction pattern

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return order;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Order> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        }

        order.Status = newStatus;

        if (newStatus == "Shipped")
        {
            order.ShippedDate = DateTime.UtcNow;
        }

        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return order;
    }

    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        return await _unitOfWork.Orders.GetPendingOrdersAsync();
    }

    public async Task ProcessOrderAsync(int orderId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get order
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found");
            }

            // Business logic: Process payment, update inventory, etc.
            // ...

            // Update order status
            order.Status = "Processing";
            await _unitOfWork.Orders.UpdateAsync(order);

            // Commit all changes
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
