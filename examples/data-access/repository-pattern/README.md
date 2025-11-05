# Repository Pattern Examples

This directory contains practical implementations of common data access patterns for Windows applications using C# and .NET.

## Overview

These examples demonstrate:
- **Repository Pattern**: Abstraction over data access
- **Unit of Work Pattern**: Transaction management
- **Entity Framework Core**: Full ORM implementation
- **Dapper**: Micro-ORM implementation
- **Database Testing**: Unit and integration test approaches

## Directory Structure

```
repository-pattern/
├── README.md (this file)
├── Models/
│   ├── User.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Product.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Repositories/
│   ├── IRepository.cs
│   ├── Repository.cs
│   ├── IUserRepository.cs
│   ├── UserRepository.cs
│   ├── IOrderRepository.cs
│   └── OrderRepository.cs
├── DapperRepositories/
│   ├── DapperUserRepository.cs
│   └── DapperOrderRepository.cs
├── UnitOfWork/
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
├── Services/
│   ├── UserService.cs
│   └── OrderService.cs
└── Tests/
    ├── UserRepositoryTests.cs
    ├── OrderRepositoryTests.cs
    └── IntegrationTests.cs
```

## Getting Started

### Prerequisites

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  <PackageReference Include="Dapper" Version="2.1.0" />
  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.0" />
  <PackageReference Include="xunit" Version="2.5.0" />
  <PackageReference Include="Moq" Version="4.20.0" />
</ItemGroup>
```

### Setup

1. **Configure connection string** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ExampleDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. **Register services** in `Program.cs`:
```csharp
// Entity Framework Core
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Repositories
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();

// Unit of Work
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
services.AddScoped<UserService>();
services.AddScoped<OrderService>();
```

3. **Create database**:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Usage Examples

### Using Repository Pattern

```csharp
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
        var user = new User
        {
            Username = username,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user);
    }
}
```

### Using Unit of Work Pattern

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Order> CreateOrderAsync(int userId, List<OrderItemDto> items)
    {
        try
        {
            // Start transaction
            await _unitOfWork.BeginTransactionAsync();

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Add order items
            foreach (var item in items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                await _unitOfWork.OrderItems.AddAsync(orderItem);
            }

            // Commit transaction
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return order;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### Using Dapper Repository

```csharp
public class DapperUserRepository
{
    private readonly string _connectionString;

    public DapperUserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        return await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT UserId, Username, Email, CreatedAt FROM Users WHERE UserId = @UserId",
            new { UserId = userId });
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        return await connection.QueryAsync<User>(
            "SELECT UserId, Username, Email, CreatedAt FROM Users WHERE IsActive = 1");
    }
}
```

## Patterns Explained

### Repository Pattern

**Purpose**: Abstracts data access logic from business logic

**Benefits**:
- Centralized data access
- Easier to test (can mock repositories)
- Decouples business logic from data access implementation
- Can switch between EF Core, Dapper, or other data access technologies

**When to Use**:
- Complex domain models
- Multiple data sources
- Need for extensive unit testing
- Team collaboration on large projects

### Unit of Work Pattern

**Purpose**: Maintains a list of objects affected by a business transaction and coordinates writing changes

**Benefits**:
- Transaction management
- Ensures data consistency
- Reduces database round-trips
- Groups related operations

**When to Use**:
- Operations span multiple repositories
- Need for explicit transaction control
- Complex business operations
- Ensuring ACID properties

### Generic Repository

**Purpose**: Provides common CRUD operations for all entities

**Benefits**:
- Code reuse
- Consistency across repositories
- Less boilerplate code

**Drawbacks**:
- Can be over-abstraction
- May not fit all entities

**Best Practice**: Combine generic repository with specific repositories for entity-specific operations

## Testing Approaches

### Unit Testing with In-Memory Database

```csharp
[Fact]
public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;

    using var context = new ApplicationDbContext(options);
    var repository = new UserRepository(context);

    var user = new User { Username = "testuser", Email = "test@example.com" };
    await repository.AddAsync(user);

    // Act
    var result = await repository.GetByIdAsync(user.UserId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("testuser", result.Username);
}
```

### Integration Testing with SQL Server

```csharp
[Fact]
public async Task CreateOrder_WithItems_SuccessfullyCommits()
{
    // Arrange
    var connectionString = _configuration.GetConnectionString("TestConnection");
    // Use test database with cleanup in constructor/dispose

    // Act
    var order = await _orderService.CreateOrderAsync(userId, items);

    // Assert
    Assert.NotEqual(0, order.OrderId);
    var savedOrder = await _orderRepository.GetByIdAsync(order.OrderId);
    Assert.NotNull(savedOrder);
    Assert.Equal(items.Count, savedOrder.OrderItems.Count);
}
```

### Mocking Repositories

```csharp
[Fact]
public async Task GetUserAsync_CallsRepository()
{
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var expectedUser = new User { UserId = 1, Username = "testuser" };
    mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedUser);

    var service = new UserService(mockRepo.Object);

    // Act
    var result = await service.GetUserAsync(1);

    // Assert
    Assert.Equal(expectedUser, result);
    mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
}
```

## Performance Considerations

### EF Core vs Dapper

**Use EF Core when**:
- Need change tracking
- Complex relationships
- Rapid development
- Migrations management

**Use Dapper when**:
- High performance is critical
- Complex SQL queries
- Read-heavy operations
- Fine-grained SQL control

### Optimization Tips

1. **Use AsNoTracking for read-only queries**:
```csharp
public async Task<List<User>> GetAllAsync()
{
    return await _context.Users
        .AsNoTracking()
        .ToListAsync();
}
```

2. **Eager load related entities**:
```csharp
public async Task<Order?> GetWithItemsAsync(int orderId)
{
    return await _context.Orders
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
        .FirstOrDefaultAsync(o => o.OrderId == orderId);
}
```

3. **Use projection for DTOs**:
```csharp
public async Task<List<UserSummaryDto>> GetUserSummariesAsync()
{
    return await _context.Users
        .Select(u => new UserSummaryDto
        {
            UserId = u.UserId,
            Username = u.Username,
            OrderCount = u.Orders.Count
        })
        .ToListAsync();
}
```

## Best Practices

1. **Keep repositories focused**: One repository per entity or aggregate root
2. **Use async/await**: All database operations should be asynchronous
3. **Handle transactions appropriately**: Use Unit of Work for multi-repository operations
4. **Don't over-abstract**: Balance between reusability and simplicity
5. **Test thoroughly**: Unit tests for logic, integration tests for data access
6. **Use appropriate tools**: EF Core for complex operations, Dapper for performance-critical queries
7. **Dispose properly**: Use `using` statements or dependency injection
8. **Document entity-specific operations**: Clear XML comments on repository methods

## Common Pitfalls

### 1. Leaking DbContext

❌ **Bad**: Long-lived DbContext
```csharp
public class Service
{
    private readonly ApplicationDbContext _context;  // Singleton!
    // Memory leaks, stale data
}
```

✅ **Good**: Scoped DbContext
```csharp
public class Service
{
    private readonly ApplicationDbContext _context;  // Scoped
    
    public Service(ApplicationDbContext context)
    {
        _context = context;  // Injected per request
    }
}
```

### 2. N+1 Queries

❌ **Bad**: Multiple queries
```csharp
var users = await _repository.GetAllAsync();
foreach (var user in users)
{
    var orders = await _orderRepository.GetByUserIdAsync(user.UserId);
}
```

✅ **Good**: Single query
```csharp
var users = await _repository.GetWithOrdersAsync();
```

### 3. Generic Repository Overuse

❌ **Bad**: Everything generic
```csharp
// Loses entity-specific logic
var activeUsers = await _genericRepository.GetAllAsync()
    .Where(u => u.IsActive);
```

✅ **Good**: Mix generic with specific
```csharp
var activeUsers = await _userRepository.GetActiveUsersAsync();
```

## Further Reading

- [Martin Fowler - Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Microsoft - Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [EF Core Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)

## Related Documentation

- [Schema Design](../../../docs/database/schema-design.md)
- [ORM Patterns](../../../docs/database/orm-patterns.md)
- [Migration Strategy](../../../docs/database/migration-strategy.md)
- [Performance Optimization](../../../docs/database/performance-optimization.md)
