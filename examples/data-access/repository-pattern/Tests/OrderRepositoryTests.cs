using Microsoft.EntityFrameworkCore;
using Xunit;
using DataAccessExamples.Data;
using DataAccessExamples.Models;
using DataAccessExamples.Repositories;

namespace DataAccessExamples.Tests;

/// <summary>
/// Unit tests for OrderRepository using in-memory database
/// </summary>
public class OrderRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _orderRepository = new OrderRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrder_WhenOrderExists()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var order = new Order
        {
            UserId = user.UserId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100.00m,
            Status = "Pending"
        };
        var addedOrder = await _orderRepository.AddAsync(order);

        // Act
        var result = await _orderRepository.GetByIdAsync(addedOrder.OrderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
        Assert.Equal(100.00m, result.TotalAmount);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserOrders()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        await _orderRepository.AddAsync(new Order { UserId = user.UserId, TotalAmount = 50m, Status = "Pending" });
        await _orderRepository.AddAsync(new Order { UserId = user.UserId, TotalAmount = 75m, Status = "Shipped" });
        
        var otherUser = await CreateTestUserAsync("other");
        await _orderRepository.AddAsync(new Order { UserId = otherUser.UserId, TotalAmount = 100m, Status = "Pending" });

        // Act
        var result = await _orderRepository.GetByUserIdAsync(user.UserId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, order => Assert.Equal(user.UserId, order.UserId));
    }

    [Fact]
    public async Task GetWithItemsAsync_ReturnsOrderWithRelatedData()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var order = await _orderRepository.AddAsync(new Order
        {
            UserId = user.UserId,
            TotalAmount = 100m,
            Status = "Pending"
        });

        // Note: In real scenario, you'd add OrderItems here
        // This test demonstrates the pattern

        // Act
        var result = await _orderRepository.GetWithItemsAsync(order.OrderId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.Equal(user.Username, result.User.Username);
        Assert.NotNull(result.OrderItems);
    }

    [Fact]
    public async Task GetPendingOrdersAsync_ReturnsOnlyPendingOrders()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        await _orderRepository.AddAsync(new Order { UserId = user.UserId, TotalAmount = 50m, Status = "Pending" });
        await _orderRepository.AddAsync(new Order { UserId = user.UserId, TotalAmount = 75m, Status = "Pending" });
        await _orderRepository.AddAsync(new Order { UserId = user.UserId, TotalAmount = 100m, Status = "Shipped" });

        // Act
        var result = await _orderRepository.GetPendingOrdersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, order => Assert.Equal("Pending", order.Status));
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsOrdersInRange()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        await _orderRepository.AddAsync(new Order 
        { 
            UserId = user.UserId, 
            OrderDate = new DateTime(2024, 1, 15),
            TotalAmount = 50m,
            Status = "Pending"
        });
        await _orderRepository.AddAsync(new Order 
        { 
            UserId = user.UserId, 
            OrderDate = new DateTime(2024, 2, 1),
            TotalAmount = 75m,
            Status = "Pending"
        });

        // Act
        var result = await _orderRepository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2024, 1, 15), result[0].OrderDate);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesOrderInDatabase()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var order = await _orderRepository.AddAsync(new Order
        {
            UserId = user.UserId,
            TotalAmount = 100m,
            Status = "Pending"
        });

        // Act
        order.Status = "Shipped";
        order.ShippedDate = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        // Assert
        var updatedOrder = await _orderRepository.GetByIdAsync(order.OrderId);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Shipped", updatedOrder.Status);
        Assert.NotNull(updatedOrder.ShippedDate);
    }

    private async Task<User> CreateTestUserAsync(string suffix = "")
    {
        var user = new User
        {
            Username = $"testuser{suffix}",
            Email = $"test{suffix}@example.com",
            IsActive = true
        };
        return await _userRepository.AddAsync(user);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
