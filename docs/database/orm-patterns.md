# ORM Usage Patterns and Data Access

## Overview

This guide covers Object-Relational Mapping (ORM) patterns for Windows applications, focusing on Entity Framework Core and Dapper, along with connection management and data access best practices.

## Table of Contents

1. [Entity Framework Core](#entity-framework-core)
2. [Dapper](#dapper)
3. [Choosing Between EF Core and Dapper](#choosing-between-ef-core-and-dapper)
4. [Connection Management](#connection-management)
5. [Connection Pooling](#connection-pooling)
6. [Repository Pattern](#repository-pattern)
7. [Unit of Work Pattern](#unit-of-work-pattern)
8. [Best Practices](#best-practices)

## Entity Framework Core

### Overview

Entity Framework Core (EF Core) is a full-featured ORM that provides:
- LINQ query support
- Change tracking
- Migrations
- Relationship management
- Lazy/eager loading

### Setup and Configuration

#### Install Packages

```xml
<!-- .csproj file -->
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
</ItemGroup>
```

#### DbContext Configuration

```csharp
using Microsoft.EntityFrameworkCore;

namespace MyApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for entities
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            
            // Configure relationship
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });

        // Seed data
        modelBuilder.Entity<OrderStatus>().HasData(
            new OrderStatus { StatusId = 1, StatusName = "Pending" },
            new OrderStatus { StatusId = 2, StatusName = "Processing" },
            new OrderStatus { StatusId = 3, StatusName = "Shipped" }
        );
    }
}
```

#### Dependency Injection Setup

```csharp
// Program.cs or Startup.cs
using Microsoft.EntityFrameworkCore;
using MyApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        }));

var app = builder.Build();
```

### Entity Configuration

#### Using Data Annotations

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Models;

[Table("Users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

#### Using Fluent API (Recommended)

```csharp
// Separate configuration class
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(e => e.UserId);
        
        builder.Property(e => e.UserId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasMany(e => e.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// Apply in OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    modelBuilder.ApplyConfiguration(new OrderConfiguration());
    // Or apply all configurations in assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}
```

### CRUD Operations

#### Create

```csharp
public async Task<User> CreateUserAsync(User user)
{
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();
    return user;
}

// Bulk insert
public async Task<int> CreateUsersAsync(IEnumerable<User> users)
{
    await _context.Users.AddRangeAsync(users);
    return await _context.SaveChangesAsync();
}
```

#### Read

```csharp
// Find by ID
public async Task<User?> GetUserByIdAsync(int userId)
{
    return await _context.Users
        .Include(u => u.Orders)  // Eager loading
        .FirstOrDefaultAsync(u => u.UserId == userId);
}

// Query with filtering
public async Task<List<User>> GetActiveUsersAsync()
{
    return await _context.Users
        .Where(u => u.IsActive)
        .OrderBy(u => u.Username)
        .ToListAsync();
}

// Projection (select specific columns)
public async Task<List<UserDto>> GetUserSummariesAsync()
{
    return await _context.Users
        .Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            OrderCount = u.Orders.Count
        })
        .ToListAsync();
}

// AsNoTracking for read-only queries (better performance)
public async Task<List<User>> GetUsersReadOnlyAsync()
{
    return await _context.Users
        .AsNoTracking()
        .ToListAsync();
}
```

#### Update

```csharp
// Update entire entity
public async Task<User> UpdateUserAsync(User user)
{
    _context.Users.Update(user);
    await _context.SaveChangesAsync();
    return user;
}

// Update specific properties
public async Task UpdateUserEmailAsync(int userId, string newEmail)
{
    var user = await _context.Users.FindAsync(userId);
    if (user != null)
    {
        user.Email = newEmail;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

// Bulk update (EF Core 7+)
public async Task<int> DeactivateOldUsersAsync(DateTime cutoffDate)
{
    return await _context.Users
        .Where(u => u.LastLoginDate < cutoffDate)
        .ExecuteUpdateAsync(u => u
            .SetProperty(x => x.IsActive, false)
            .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
}
```

#### Delete

```csharp
// Delete by entity
public async Task DeleteUserAsync(User user)
{
    _context.Users.Remove(user);
    await _context.SaveChangesAsync();
}

// Delete by ID
public async Task DeleteUserByIdAsync(int userId)
{
    var user = await _context.Users.FindAsync(userId);
    if (user != null)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}

// Bulk delete (EF Core 7+)
public async Task<int> DeleteInactiveUsersAsync()
{
    return await _context.Users
        .Where(u => !u.IsActive)
        .ExecuteDeleteAsync();
}
```

### Advanced Queries

#### Pagination

```csharp
public async Task<PagedResult<User>> GetUsersPagedAsync(int page, int pageSize)
{
    var query = _context.Users.AsNoTracking();
    
    var totalCount = await query.CountAsync();
    
    var users = await query
        .OrderBy(u => u.Username)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<User>
    {
        Items = users,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

#### Complex Queries

```csharp
// Join multiple tables
public async Task<List<OrderSummaryDto>> GetOrderSummariesAsync()
{
    return await _context.Orders
        .Include(o => o.User)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
        .Select(o => new OrderSummaryDto
        {
            OrderId = o.OrderId,
            Username = o.User.Username,
            TotalAmount = o.TotalAmount,
            ItemCount = o.OrderItems.Count,
            ProductNames = o.OrderItems.Select(oi => oi.Product.ProductName).ToList()
        })
        .ToListAsync();
}

// Group by and aggregate
public async Task<List<UserOrderStatistics>> GetUserOrderStatsAsync()
{
    return await _context.Orders
        .GroupBy(o => o.UserId)
        .Select(g => new UserOrderStatistics
        {
            UserId = g.Key,
            OrderCount = g.Count(),
            TotalSpent = g.Sum(o => o.TotalAmount),
            AverageOrderValue = g.Average(o => o.TotalAmount)
        })
        .ToListAsync();
}
```

### Migrations

```bash
# Add a new migration
dotnet ef migrations add InitialCreate

# Update database to latest migration
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Generate SQL script
dotnet ef migrations script

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Performance Tips

```csharp
// 1. Use AsNoTracking for read-only queries
var users = await _context.Users.AsNoTracking().ToListAsync();

// 2. Use projection to select only needed columns
var userNames = await _context.Users
    .Select(u => u.Username)
    .ToListAsync();

// 3. Use Include for eager loading to avoid N+1 queries
var users = await _context.Users
    .Include(u => u.Orders)
    .ToListAsync();

// 4. Split large queries
var userIds = await _context.Users
    .Where(u => u.IsActive)
    .Select(u => u.UserId)
    .ToListAsync();

var orders = await _context.Orders
    .Where(o => userIds.Contains(o.UserId))
    .ToListAsync();

// 5. Use compiled queries for frequently-run queries
private static readonly Func<ApplicationDbContext, int, Task<User?>> GetUserById =
    EF.CompileAsyncQuery((ApplicationDbContext context, int userId) =>
        context.Users.FirstOrDefault(u => u.UserId == userId));

public async Task<User?> GetUserAsync(int userId)
{
    return await GetUserById(_context, userId);
}
```

## Dapper

### Overview

Dapper is a lightweight micro-ORM that provides:
- High performance (close to raw ADO.NET)
- Simple API
- Support for stored procedures
- Multi-mapping capabilities

### Setup

```xml
<!-- .csproj file -->
<ItemGroup>
  <PackageReference Include="Dapper" Version="2.1.0" />
  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.0" />
</ItemGroup>
```

### Basic Operations

```csharp
using Dapper;
using Microsoft.Data.SqlClient;

namespace MyApp.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Query single entity
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "SELECT UserId, Username, Email, CreatedAt FROM Users WHERE UserId = @UserId";
        
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    // Query multiple entities
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "SELECT UserId, Username, Email, CreatedAt FROM Users";
        
        return await connection.QueryAsync<User>(sql);
    }

    // Execute command (INSERT, UPDATE, DELETE)
    public async Task<int> CreateUserAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            INSERT INTO Users (Username, Email, CreatedAt)
            VALUES (@Username, @Email, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";
        
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    // Execute update
    public async Task<int> UpdateUserAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            UPDATE Users 
            SET Username = @Username, 
                Email = @Email, 
                UpdatedAt = @UpdatedAt
            WHERE UserId = @UserId";
        
        return await connection.ExecuteAsync(sql, user);
    }

    // Execute delete
    public async Task<int> DeleteUserAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "DELETE FROM Users WHERE UserId = @UserId";
        
        return await connection.ExecuteAsync(sql, new { UserId = userId });
    }
}
```

### Advanced Queries

#### Multi-Mapping (Join Queries)

```csharp
public async Task<IEnumerable<Order>> GetOrdersWithUsersAsync()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        SELECT o.OrderId, o.OrderDate, o.TotalAmount,
               u.UserId, u.Username, u.Email
        FROM Orders o
        INNER JOIN Users u ON o.UserId = u.UserId";
    
    return await connection.QueryAsync<Order, User, Order>(
        sql,
        (order, user) =>
        {
            order.User = user;
            return order;
        },
        splitOn: "UserId");
}

// Multi-level mapping
public async Task<IEnumerable<Order>> GetOrdersWithItemsAsync()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        SELECT o.OrderId, o.OrderDate, o.TotalAmount,
               oi.OrderItemId, oi.Quantity, oi.UnitPrice,
               p.ProductId, p.ProductName
        FROM Orders o
        INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
        INNER JOIN Products p ON oi.ProductId = p.ProductId
        ORDER BY o.OrderId";
    
    var orderDict = new Dictionary<int, Order>();
    
    await connection.QueryAsync<Order, OrderItem, Product, Order>(
        sql,
        (order, orderItem, product) =>
        {
            if (!orderDict.TryGetValue(order.OrderId, out var currentOrder))
            {
                currentOrder = order;
                currentOrder.OrderItems = new List<OrderItem>();
                orderDict.Add(order.OrderId, currentOrder);
            }
            
            orderItem.Product = product;
            currentOrder.OrderItems.Add(orderItem);
            
            return currentOrder;
        },
        splitOn: "OrderItemId,ProductId");
    
    return orderDict.Values;
}
```

#### Stored Procedures

```csharp
public async Task<IEnumerable<User>> GetUsersByStoredProcAsync(string searchTerm)
{
    using var connection = new SqlConnection(_connectionString);
    
    return await connection.QueryAsync<User>(
        "sp_SearchUsers",
        new { SearchTerm = searchTerm },
        commandType: CommandType.StoredProcedure);
}

// Execute stored procedure with output parameters
public async Task<(int userId, string message)> CreateUserWithOutputAsync(User user)
{
    using var connection = new SqlConnection(_connectionString);
    
    var parameters = new DynamicParameters();
    parameters.Add("@Username", user.Username);
    parameters.Add("@Email", user.Email);
    parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
    parameters.Add("@Message", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);
    
    await connection.ExecuteAsync(
        "sp_CreateUser",
        parameters,
        commandType: CommandType.StoredProcedure);
    
    return (
        parameters.Get<int>("@UserId"),
        parameters.Get<string>("@Message")
    );
}
```

#### Bulk Operations

```csharp
// Bulk insert with transaction
public async Task<int> BulkInsertUsersAsync(IEnumerable<User> users)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    
    using var transaction = connection.BeginTransaction();
    try
    {
        var sql = @"
            INSERT INTO Users (Username, Email, CreatedAt)
            VALUES (@Username, @Email, @CreatedAt)";
        
        var result = await connection.ExecuteAsync(sql, users, transaction);
        
        transaction.Commit();
        return result;
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

### Dapper Contrib (Extensions)

```csharp
using Dapper.Contrib.Extensions;

// Simplified CRUD operations
[Table("Users")]
public class User
{
    [Key]
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UserRepository
{
    private readonly string _connectionString;

    // Insert
    public async Task<int> InsertAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.InsertAsync(user);
    }

    // Update
    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.UpdateAsync(user);
    }

    // Delete
    public async Task<bool> DeleteAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.DeleteAsync(user);
    }

    // Get by ID
    public async Task<User?> GetAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.GetAsync<User>(userId);
    }

    // Get all
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.GetAllAsync<User>();
    }
}
```

## Choosing Between EF Core and Dapper

### Use Entity Framework Core When:

✅ You need full ORM features (change tracking, migrations)
✅ Your application has complex relationships
✅ You prefer LINQ query syntax
✅ Development speed is more important than raw performance
✅ You want automatic migrations and schema management
✅ The project is complex with many entities

### Use Dapper When:

✅ Performance is critical
✅ You need fine control over SQL
✅ You're working with stored procedures
✅ You have simple data access patterns
✅ You want minimal overhead
✅ The project is simpler or performance-sensitive

### Hybrid Approach

```csharp
// Use both in the same application
public class OrderService
{
    private readonly ApplicationDbContext _efContext;
    private readonly string _connectionString;

    public OrderService(ApplicationDbContext efContext, IConfiguration configuration)
    {
        _efContext = efContext;
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    // Use EF Core for complex writes with relationships
    public async Task<Order> CreateOrderAsync(Order order)
    {
        await _efContext.Orders.AddAsync(order);
        await _efContext.SaveChangesAsync();
        return order;
    }

    // Use Dapper for high-performance reads
    public async Task<IEnumerable<OrderSummaryDto>> GetOrderSummariesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT o.OrderId, o.OrderDate, u.Username, o.TotalAmount
            FROM Orders o
            INNER JOIN Users u ON o.UserId = u.UserId
            ORDER BY o.OrderDate DESC";
        
        return await connection.QueryAsync<OrderSummaryDto>(sql);
    }
}
```

## Connection Management

### Connection String Configuration

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyAppDb;Trusted_Connection=true;MultipleActiveResultSets=true",
    "ReadOnlyConnection": "Server=readonly-server;Database=MyAppDb;User Id=reader;Password=***;",
    "ProductionConnection": "Server=prod-server;Database=MyAppDb;User Id=app_user;Password=***;Encrypt=true;TrustServerCertificate=false;"
  }
}

// Access in code
var connectionString = configuration.GetConnectionString("DefaultConnection");
```

### Connection Lifetime

#### With Entity Framework Core

```csharp
// Scoped lifetime (recommended for web applications)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);

// Using the context
public class OrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;  // Injected, managed by DI container
    }

    public async Task<Order?> GetOrderAsync(int orderId)
    {
        return await _context.Orders.FindAsync(orderId);
        // Connection automatically opened and closed
    }
}
```

#### With Dapper

```csharp
// Always use 'using' to ensure disposal
public async Task<User?> GetUserAsync(int userId)
{
    using var connection = new SqlConnection(_connectionString);
    // Connection automatically opened by Dapper if needed
    return await connection.QuerySingleOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE UserId = @UserId",
        new { UserId = userId });
    // Connection automatically closed and disposed
}

// For multiple operations, explicitly open
public async Task<(List<User> users, int count)> GetUsersWithCountAsync()
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    
    var users = await connection.QueryAsync<User>("SELECT * FROM Users");
    var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
    
    return (users.ToList(), count);
}
```

### Connection Resiliency

```csharp
// EF Core - Configure retry logic
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Dapper - Implement retry policy with Polly
public class ResilientDapperRepository
{
    private readonly string _connectionString;
    private readonly IAsyncPolicy _retryPolicy;

    public ResilientDapperRepository(string connectionString)
    {
        _connectionString = connectionString;
        
        _retryPolicy = Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Logger.Warning($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to {exception.Message}");
                });
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserId = @UserId",
                new { UserId = userId });
        });
    }
}
```

## Connection Pooling

### SQL Server Connection Pooling

Connection pooling is automatic in ADO.NET/SQL Server:

```csharp
// Connection string with pooling settings
"Server=myserver;Database=mydb;User Id=user;Password=pwd;" +
"Min Pool Size=5;" +          // Minimum connections in pool
"Max Pool Size=100;" +         // Maximum connections in pool
"Connection Lifetime=300;" +   // Max time (seconds) before connection is destroyed
"Connection Timeout=30;" +     // Timeout for connection attempts
"Pooling=true"                 // Enable pooling (default)
```

### Best Practices

```csharp
// ✅ Good: Connections returned to pool quickly
public async Task<User?> GetUserAsync(int userId)
{
    using var connection = new SqlConnection(_connectionString);
    return await connection.QuerySingleOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE UserId = @UserId",
        new { UserId = userId });
    // Connection returned to pool here
}

// ❌ Bad: Connection held for long time
public async Task<User?> GetUserSlowAsync(int userId)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    
    // Don't do expensive work while holding connection
    await DoExpensiveWorkAsync();  // Bad!
    
    return await connection.QuerySingleOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE UserId = @UserId",
        new { UserId = userId });
}

// ✅ Good: Do expensive work before/after database operations
public async Task<User?> GetUserBetterAsync(int userId)
{
    await DoExpensiveWorkAsync();  // Do this first
    
    using var connection = new SqlConnection(_connectionString);
    var user = await connection.QuerySingleOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE UserId = @UserId",
        new { UserId = userId });
    
    await DoMoreExpensiveWorkAsync(user);  // Do this after
    
    return user;
}
```

### Monitoring Connection Pool

```csharp
// Get pool statistics (for debugging)
public class ConnectionPoolMonitor
{
    public static void LogPoolStatistics(string connectionString)
    {
        var pool = SqlConnection.GetPool(connectionString);
        // Note: In production, use performance counters instead
        Logger.Info($"Pool size: {pool.Count}");
    }
}
```

## Repository Pattern

See the complete implementation in [/examples/data-access/repository-pattern/](../../examples/data-access/repository-pattern/).

### Basic Pattern

```csharp
// Generic repository interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Implementation
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
```

## Unit of Work Pattern

See the complete implementation in [/examples/data-access/repository-pattern/](../../examples/data-access/repository-pattern/).

```csharp
// Unit of Work interface
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Implementation
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public IOrderRepository Orders { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Orders = new OrderRepository(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

## Best Practices

### 1. Always Use Async/Await

```csharp
// ✅ Good
public async Task<User?> GetUserAsync(int userId)
{
    return await _context.Users.FindAsync(userId);
}

// ❌ Bad
public User? GetUser(int userId)
{
    return _context.Users.Find(userId);  // Synchronous
}
```

### 2. Dispose Database Contexts and Connections

```csharp
// ✅ Good: Using statement ensures disposal
using (var context = new ApplicationDbContext(options))
{
    var users = await context.Users.ToListAsync();
}

// ✅ Good: DI container manages lifetime
public class UserService
{
    private readonly ApplicationDbContext _context;  // Disposed by DI
    
    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }
}
```

### 3. Use Transactions for Multiple Operations

```csharp
public async Task TransferOrderAsync(int orderId, int newUserId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.UserId = newUserId;
            await _context.SaveChangesAsync();
        }

        var notification = new Notification
        {
            UserId = newUserId,
            Message = $"Order {orderId} transferred to you"
        };
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### 4. Handle Concurrency Conflicts

```csharp
// Add RowVersion to entity
public class Order
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

// Handle conflicts
public async Task<bool> UpdateOrderAsync(Order order)
{
    try
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return true;
    }
    catch (DbUpdateConcurrencyException ex)
    {
        foreach (var entry in ex.Entries)
        {
            if (entry.Entity is Order)
            {
                var databaseValues = await entry.GetDatabaseValuesAsync();
                if (databaseValues == null)
                {
                    // Record was deleted
                    return false;
                }
                
                // Reload and retry, or merge changes
                entry.OriginalValues.SetValues(databaseValues);
            }
        }
        
        // Retry
        await _context.SaveChangesAsync();
        return true;
    }
}
```

### 5. Use Parameterized Queries

```csharp
// ✅ Good: Parameterized (safe from SQL injection)
var users = await _context.Users
    .Where(u => u.Username == username)
    .ToListAsync();

// ✅ Good with Dapper
await connection.QueryAsync<User>(
    "SELECT * FROM Users WHERE Username = @Username",
    new { Username = username });

// ❌ Bad: String concatenation (SQL injection risk!)
var sql = $"SELECT * FROM Users WHERE Username = '{username}'";
```

### 6. Log Database Operations

```csharp
// Configure logging in EF Core
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging()  // Only in development
           .EnableDetailedErrors();        // Only in development
});

// Custom logging
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.LogTo(
        message => Logger.Debug(message),
        new[] { DbLoggerCategory.Database.Command.Name },
        LogLevel.Information);
}
```

## Related Documents

- [Schema Design](./schema-design.md)
- [Migration Strategy](./migration-strategy.md)
- [Performance Optimization](./performance-optimization.md)
- [Repository Pattern Examples](../../examples/data-access/repository-pattern/)
