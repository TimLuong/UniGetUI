# Database Performance Optimization

## Overview

This guide covers query optimization techniques, caching strategies, and performance best practices for Windows applications using SQL Server, Entity Framework Core, and Dapper.

## Table of Contents

1. [Query Optimization](#query-optimization)
2. [Indexing Strategies](#indexing-strategies)
3. [Entity Framework Core Performance](#entity-framework-core-performance)
4. [Caching Strategies](#caching-strategies)
5. [Connection Pool Optimization](#connection-pool-optimization)
6. [Monitoring and Profiling](#monitoring-and-profiling)
7. [Common Performance Anti-Patterns](#common-performance-anti-patterns)
8. [Best Practices](#best-practices)

## Query Optimization

### Use Appropriate Query Patterns

#### Select Only Required Columns

```csharp
// ❌ Bad: Select all columns
var users = await _context.Users.ToListAsync();

// ✅ Good: Project only needed columns
var userSummaries = await _context.Users
    .Select(u => new UserSummaryDto
    {
        UserId = u.UserId,
        Username = u.Username,
        Email = u.Email
    })
    .ToListAsync();

// With Dapper
// ❌ Bad
var users = await connection.QueryAsync<User>("SELECT * FROM Users");

// ✅ Good
var users = await connection.QueryAsync<UserSummary>(
    "SELECT UserId, Username, Email FROM Users");
```

### Avoid N+1 Query Problems

```csharp
// ❌ Bad: N+1 queries (1 for users + N for orders)
var users = await _context.Users.ToListAsync();
foreach (var user in users)
{
    // Each iteration causes a separate database query!
    var orders = await _context.Orders
        .Where(o => o.UserId == user.UserId)
        .ToListAsync();
}

// ✅ Good: Single query with Include
var users = await _context.Users
    .Include(u => u.Orders)
    .ToListAsync();

// ✅ Good: Load separately and join in memory
var users = await _context.Users.ToListAsync();
var userIds = users.Select(u => u.UserId).ToList();
var orders = await _context.Orders
    .Where(o => userIds.Contains(o.UserId))
    .ToListAsync();

// Group orders by user in memory
var ordersByUser = orders.GroupBy(o => o.UserId).ToDictionary(g => g.Key, g => g.ToList());
```

### Use AsNoTracking for Read-Only Queries

```csharp
// ❌ Slower: Change tracking enabled (default)
var users = await _context.Users.ToListAsync();

// ✅ Faster: No change tracking for read-only operations
var users = await _context.Users
    .AsNoTracking()
    .ToListAsync();

// Can improve performance by 30-50% for read-only queries
```

### Pagination

```csharp
// ✅ Efficient pagination
public async Task<PagedResult<User>> GetUsersPagedAsync(int page, int pageSize)
{
    var query = _context.Users.AsNoTracking();
    
    var totalCount = await query.CountAsync();
    
    var users = await query
        .OrderBy(u => u.UserId)  // Important: Always order for consistent pagination
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<User>
    {
        Items = users,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}

// With Dapper (more efficient - single query)
public async Task<PagedResult<User>> GetUsersPagedDapperAsync(int page, int pageSize)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        SELECT COUNT(*) FROM Users;
        
        SELECT UserId, Username, Email
        FROM Users
        ORDER BY UserId
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
    
    using var multi = await connection.QueryMultipleAsync(sql, 
        new { Offset = (page - 1) * pageSize, PageSize = pageSize });
    
    var totalCount = await multi.ReadSingleAsync<int>();
    var users = await multi.ReadAsync<User>();
    
    return new PagedResult<User>
    {
        Items = users.ToList(),
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}
```

### Filtering and Searching

```csharp
// ✅ Efficient filtering
public async Task<List<User>> SearchUsersAsync(string searchTerm, bool? isActive = null)
{
    var query = _context.Users.AsNoTracking();
    
    // Apply filters conditionally
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        query = query.Where(u => 
            u.Username.Contains(searchTerm) || 
            u.Email.Contains(searchTerm));
    }
    
    if (isActive.HasValue)
    {
        query = query.Where(u => u.IsActive == isActive.Value);
    }
    
    return await query
        .OrderBy(u => u.Username)
        .ToListAsync();
}

// ✅ Full-text search for large text columns
public async Task<List<Product>> FullTextSearchAsync(string searchTerm)
{
    // Requires full-text index on Description column
    return await _context.Products
        .FromSqlRaw(@"
            SELECT * FROM Products
            WHERE CONTAINS(Description, {0})", searchTerm)
        .ToListAsync();
}
```

### Bulk Operations

```csharp
// ❌ Bad: Individual operations (slow for large datasets)
foreach (var user in users)
{
    _context.Users.Add(user);
    await _context.SaveChangesAsync();  // N database round-trips!
}

// ✅ Good: Batch operations
_context.Users.AddRange(users);
await _context.SaveChangesAsync();  // Single round-trip

// ✅ Better: Use ExecuteUpdate/ExecuteDelete (EF Core 7+)
await _context.Users
    .Where(u => u.LastLoginDate < DateTime.UtcNow.AddYears(-1))
    .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsActive, false));

// ✅ Best: Bulk operations with Dapper or SqlBulkCopy
public async Task BulkInsertAsync(IEnumerable<User> users)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    
    using var bulkCopy = new SqlBulkCopy(connection);
    bulkCopy.DestinationTableName = "Users";
    bulkCopy.BatchSize = 1000;
    
    var dataTable = ToDataTable(users);
    await bulkCopy.WriteToServerAsync(dataTable);
}
```

### Compiled Queries

```csharp
// Pre-compile frequently-used queries for better performance
private static readonly Func<ApplicationDbContext, int, Task<User?>> GetUserByIdCompiled =
    EF.CompileAsyncQuery((ApplicationDbContext context, int userId) =>
        context.Users.FirstOrDefault(u => u.UserId == userId));

private static readonly Func<ApplicationDbContext, string, Task<List<User>>> GetUsersByNameCompiled =
    EF.CompileAsyncQuery((ApplicationDbContext context, string name) =>
        context.Users.Where(u => u.Username.Contains(name)).ToList());

public async Task<User?> GetUserByIdAsync(int userId)
{
    // 30-50% faster for frequently-called queries
    return await GetUserByIdCompiled(_context, userId);
}

public async Task<List<User>> GetUsersByNameAsync(string name)
{
    return await GetUsersByNameCompiled(_context, name);
}
```

## Indexing Strategies

### Index Selection

```sql
-- Index foreign keys (critical for joins)
CREATE INDEX IX_Orders_UserId ON Orders(UserId);

-- Index columns used in WHERE clauses
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

-- Composite indexes for common query patterns
-- Query: WHERE UserId = @UserId ORDER BY OrderDate DESC
CREATE INDEX IX_Orders_UserId_OrderDate ON Orders(UserId, OrderDate DESC);

-- Covering index (includes non-key columns)
CREATE INDEX IX_Orders_UserId_Covering 
ON Orders(UserId) 
INCLUDE (OrderDate, TotalAmount, Status);

-- Filtered index (for subset of data)
CREATE INDEX IX_Orders_Active 
ON Orders(OrderDate)
WHERE Status = 'Active';
```

### Index Configuration in EF Core

```csharp
// Configure indexes in OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Single column index
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();

    // Composite index
    modelBuilder.Entity<Order>()
        .HasIndex(o => new { o.UserId, o.OrderDate })
        .HasDatabaseName("IX_Orders_UserId_OrderDate");

    // Filtered index
    modelBuilder.Entity<Order>()
        .HasIndex(o => o.OrderDate)
        .HasFilter("Status = 'Active'")
        .HasDatabaseName("IX_Orders_Active");

    // Descending index (EF Core 5+)
    modelBuilder.Entity<Order>()
        .HasIndex(o => o.OrderDate)
        .IsDescending();
}
```

### Index Maintenance

```sql
-- Check index usage
SELECT 
    OBJECT_NAME(s.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE OBJECT_NAME(s.object_id) = 'Users'
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;

-- Find missing indexes
SELECT 
    migs.avg_user_impact * (migs.user_seeks + migs.user_scans) AS Impact,
    mid.statement AS TableName,
    mid.equality_columns,
    mid.inequality_columns,
    mid.included_columns
FROM sys.dm_db_missing_index_details mid
INNER JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
INNER JOIN sys.dm_db_missing_index_group_stats migs ON mig.index_group_handle = migs.group_handle
ORDER BY Impact DESC;

-- Rebuild fragmented indexes
ALTER INDEX IX_Users_Email ON Users REBUILD;

-- Update statistics
UPDATE STATISTICS Users;
```

## Entity Framework Core Performance

### Eager vs Lazy vs Explicit Loading

```csharp
// Eager Loading (single query with JOIN)
var users = await _context.Users
    .Include(u => u.Orders)
        .ThenInclude(o => o.OrderItems)
    .ToListAsync();

// Explicit Loading (separate queries, but controlled)
var user = await _context.Users.FindAsync(userId);
await _context.Entry(user)
    .Collection(u => u.Orders)
    .LoadAsync();

// Lazy Loading (automatic, but can cause N+1 issues)
// Enable in OnConfiguring:
// optionsBuilder.UseLazyLoadingProxies();
// Mark navigation properties as virtual
public virtual ICollection<Order> Orders { get; set; }
```

### Query Splitting

```csharp
// For queries with multiple collections, avoid cartesian explosion
var users = await _context.Users
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .AsSplitQuery()  // Executes as separate queries
    .ToListAsync();

// Configure globally
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlServer(connectionString)
        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
}
```

### Tracking vs No-Tracking

```csharp
// Change tracking overhead comparison
Stopwatch sw = Stopwatch.StartNew();

// With tracking
var tracked = await _context.Users.ToListAsync();
Console.WriteLine($"With tracking: {sw.ElapsedMilliseconds}ms");

sw.Restart();

// Without tracking (30-50% faster)
var noTracking = await _context.Users.AsNoTracking().ToListAsync();
Console.WriteLine($"No tracking: {sw.ElapsedMilliseconds}ms");

// Configure globally for read-only contexts
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlServer(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}
```

### Batch Size Configuration

```csharp
// Configure batch size for bulk operations
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlServer(connectionString, 
            options => options.MaxBatchSize(100));  // Default is 42
}

// Larger batch size = fewer round-trips, but larger packets
// Optimal: 100-1000 depending on row size
```

### DbContext Lifetime

```csharp
// ❌ Bad: Long-lived DbContext
public class BadService
{
    private readonly ApplicationDbContext _context;  // Singleton lifetime
    
    public BadService(ApplicationDbContext context)
    {
        _context = context;  // Memory leaks, stale data
    }
}

// ✅ Good: Scoped DbContext (one per request in web apps)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);

// ✅ Good: Short-lived for console apps
public async Task ProcessDataAsync()
{
    using var context = new ApplicationDbContext(options);
    
    // Do work with context
    var users = await context.Users.ToListAsync();
    
    // Context disposed automatically
}
```

## Caching Strategies

### In-Memory Caching

```csharp
// Setup in Program.cs
services.AddMemoryCache();

// Service implementation
public class UserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string UsersCacheKey = "all_users";

    public UserService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        // Try get from cache
        if (_cache.TryGetValue(UsersCacheKey, out List<User>? cachedUsers))
        {
            return cachedUsers!;
        }

        // Not in cache, fetch from database
        var users = await _context.Users.AsNoTracking().ToListAsync();

        // Set cache options
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.Normal
        };

        // Save to cache
        _cache.Set(UsersCacheKey, users, cacheOptions);

        return users;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove(UsersCacheKey);

        return user;
    }
}
```

### Distributed Caching (Redis)

```csharp
// Install packages
// Microsoft.Extensions.Caching.StackExchangeRedis

// Setup in Program.cs
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp:";
});

// Service implementation
public class UserService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public UserService(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var cacheKey = $"user:{userId}";

        // Try get from cache
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<User>(cachedData);
        }

        // Not in cache, fetch from database
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user != null)
        {
            // Save to cache
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var serialized = JsonSerializer.Serialize(user);
            await _cache.SetStringAsync(cacheKey, serialized, options);
        }

        return user;
    }

    public async Task InvalidateUserCacheAsync(int userId)
    {
        await _cache.RemoveAsync($"user:{userId}");
    }
}
```

### Query Result Caching

```csharp
// Cache query results with automatic invalidation
public class CachedQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public async Task<List<TResult>> GetCachedQueryAsync<TResult>(
        string cacheKey,
        Func<Task<List<TResult>>> query,
        TimeSpan? cacheDuration = null)
    {
        if (_cache.TryGetValue(cacheKey, out List<TResult>? cached))
        {
            return cached!;
        }

        var result = await query();

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheDuration ?? TimeSpan.FromMinutes(5)
        };

        _cache.Set(cacheKey, result, options);

        return result;
    }
}

// Usage
public async Task<List<User>> GetActiveUsersAsync()
{
    return await _cachedQueryService.GetCachedQueryAsync(
        "active_users",
        async () => await _context.Users
            .Where(u => u.IsActive)
            .AsNoTracking()
            .ToListAsync(),
        TimeSpan.FromMinutes(10));
}
```

### Cache-Aside Pattern

```csharp
public class CacheAsidePattern<T>
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

    public async Task<T?> GetAsync(string key, Func<Task<T?>> fetchFromSource)
    {
        // 1. Try cache
        var cachedData = await _cache.GetStringAsync(key);
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        // 2. Cache miss - fetch from source
        var data = await fetchFromSource();
        if (data == null) return default;

        // 3. Write to cache
        await SetAsync(key, data);

        return data;
    }

    public async Task SetAsync(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };

        var serialized = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serialized, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}

// Usage
public async Task<User?> GetUserAsync(int userId)
{
    return await _cacheAside.GetAsync(
        $"user:{userId}",
        async () => await _context.Users.FindAsync(userId));
}
```

## Connection Pool Optimization

### Connection String Configuration

```csharp
// Optimized connection string
var connectionString = "Server=myserver;Database=mydb;User Id=user;Password=pwd;" +
    "Min Pool Size=5;" +           // Minimum connections maintained
    "Max Pool Size=100;" +          // Maximum connections allowed
    "Connection Lifetime=300;" +    // Recycle connections after 5 minutes
    "Connection Timeout=30;" +      // Timeout for connection attempts
    "Pooling=true;" +               // Enable pooling
    "MultipleActiveResultSets=true"; // MARS for async operations
```

### Best Practices

```csharp
// ✅ Good: Return connections quickly
public async Task<User?> GetUserAsync(int userId)
{
    using var connection = new SqlConnection(_connectionString);
    return await connection.QuerySingleOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE UserId = @UserId",
        new { UserId = userId });
    // Connection returned to pool immediately
}

// ❌ Bad: Hold connection during expensive operation
public async Task<User?> GetUserSlowAsync(int userId)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    
    var user = await connection.QuerySingleOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE UserId = @UserId",
        new { UserId = userId });
    
    // Don't hold connection during expensive work!
    await SendEmailAsync(user);  // ❌ Bad
    await ProcessImageAsync(user); // ❌ Bad
    
    return user;
}

// ✅ Good: Release connection before expensive work
public async Task<User?> GetUserBetterAsync(int userId)
{
    User? user;
    using (var connection = new SqlConnection(_connectionString))
    {
        user = await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE UserId = @UserId",
            new { UserId = userId });
    }  // Connection returned to pool here
    
    // Do expensive work after releasing connection
    if (user != null)
    {
        await SendEmailAsync(user);
        await ProcessImageAsync(user);
    }
    
    return user;
}
```

## Monitoring and Profiling

### SQL Server Query Profiling

```sql
-- Enable Query Store
ALTER DATABASE MyAppDb SET QUERY_STORE = ON;

-- View expensive queries
SELECT TOP 10
    q.query_id,
    qt.query_sql_text,
    rs.avg_duration / 1000 AS avg_duration_ms,
    rs.avg_logical_io_reads,
    rs.count_executions
FROM sys.query_store_query q
INNER JOIN sys.query_store_query_text qt ON q.query_text_id = qt.query_text_id
INNER JOIN sys.query_store_plan p ON q.query_id = p.query_id
INNER JOIN sys.query_store_runtime_stats rs ON p.plan_id = rs.plan_id
ORDER BY rs.avg_duration DESC;
```

### EF Core Query Logging

```csharp
// Configure logging
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()  // Development only!
        .EnableDetailedErrors();       // Development only!
}

// Custom logging
public class QueryLogger
{
    public static void ConfigureLogging(DbContextOptionsBuilder options, ILogger logger)
    {
        options.LogTo(
            message =>
            {
                if (message.Contains("Executed DbCommand"))
                {
                    // Parse and log slow queries
                    if (ParseExecutionTime(message) > 1000)  // > 1 second
                    {
                        logger.LogWarning($"Slow query detected: {message}");
                    }
                }
            },
            new[] { DbLoggerCategory.Database.Command.Name },
            LogLevel.Information);
    }

    private static int ParseExecutionTime(string message)
    {
        var match = Regex.Match(message, @"\[(\d+)ms\]");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
}
```

### Application Insights Integration

```csharp
// Install: Microsoft.ApplicationInsights.AspNetCore

services.AddApplicationInsightsTelemetry(configuration["ApplicationInsights:InstrumentationKey"]);

// Automatically tracks:
// - Database queries and execution time
// - Dependency calls
// - Exceptions
// - Performance counters

// Custom tracking
public class OrderService
{
    private readonly TelemetryClient _telemetry;

    public async Task<Order> CreateOrderAsync(Order order)
    {
        using var operation = _telemetry.StartOperation<DependencyTelemetry>("CreateOrder");
        
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            
            stopwatch.Stop();
            
            _telemetry.TrackMetric("OrderCreationTime", stopwatch.ElapsedMilliseconds);
            
            return order;
        }
        catch (Exception ex)
        {
            _telemetry.TrackException(ex);
            throw;
        }
    }
}
```

## Common Performance Anti-Patterns

### 1. SELECT N+1 Problem

```csharp
// ❌ Anti-pattern
var users = await _context.Users.ToListAsync();
foreach (var user in users)
{
    var orders = await _context.Orders
        .Where(o => o.UserId == user.UserId)
        .ToListAsync();  // N additional queries!
}

// ✅ Solution
var users = await _context.Users
    .Include(u => u.Orders)
    .ToListAsync();  // Single query with JOIN
```

### 2. Loading Too Much Data

```csharp
// ❌ Anti-pattern
var allUsers = await _context.Users.ToListAsync();  // Load everything
var activeUsers = allUsers.Where(u => u.IsActive).ToList();  // Filter in memory

// ✅ Solution
var activeUsers = await _context.Users
    .Where(u => u.IsActive)
    .ToListAsync();  // Filter in database
```

### 3. Not Using Indexes

```csharp
// ❌ Anti-pattern: Query on non-indexed column
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.SocialSecurityNumber == ssn);  // Table scan!

// ✅ Solution: Add index
modelBuilder.Entity<User>()
    .HasIndex(u => u.SocialSecurityNumber);
```

### 4. Using GUID Primary Keys

```csharp
// ❌ Problematic: GUID primary keys cause fragmentation
public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
}

// ✅ Better: Sequential GUID or INT identity
public class Order
{
    public int OrderId { get; set; }  // Identity column
}

// Or use sequential GUID
public static Guid NewSequentialGuid()
{
    return Microsoft.Data.SqlClient.SqlGuid.NewGuid().Value;
}
```

### 5. Not Disposing Contexts

```csharp
// ❌ Anti-pattern: Context leak
public class BadService
{
    private ApplicationDbContext _context;  // Never disposed!
    
    public void LoadData()
    {
        _context = new ApplicationDbContext();
        var users = _context.Users.ToList();
    }
}

// ✅ Solution: Always dispose
public async Task LoadDataAsync()
{
    using var context = new ApplicationDbContext();
    var users = await context.Users.ToListAsync();
}  // Disposed automatically
```

## Best Practices

### 1. Measure Before Optimizing

```csharp
public class PerformanceTest
{
    public async Task CompareApproachesAsync()
    {
        // Approach 1
        var sw1 = Stopwatch.StartNew();
        var result1 = await GetUsersApproach1();
        sw1.Stop();
        Console.WriteLine($"Approach 1: {sw1.ElapsedMilliseconds}ms");

        // Approach 2
        var sw2 = Stopwatch.StartNew();
        var result2 = await GetUsersApproach2();
        sw2.Stop();
        Console.WriteLine($"Approach 2: {sw2.ElapsedMilliseconds}ms");
    }
}
```

### 2. Use Appropriate Tools

- **Development**: EF Core with logging
- **Performance Testing**: Dapper or EF Core with AsNoTracking
- **Bulk Operations**: SqlBulkCopy or library like EFCore.BulkExtensions
- **Reporting**: Dapper with raw SQL
- **OLTP**: EF Core
- **OLAP**: Dapper or stored procedures

### 3. Cache Strategically

```csharp
// Cache reference data (changes rarely)
public async Task<List<Country>> GetCountriesAsync()
{
    // Cache for 24 hours
    return await _cache.GetOrCreateAsync("countries", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
        return await _context.Countries.AsNoTracking().ToListAsync();
    });
}

// Don't cache transactional data (changes frequently)
public async Task<Order> GetOrderAsync(int orderId)
{
    // Always fetch fresh data
    return await _context.Orders.FindAsync(orderId);
}
```

### 4. Optimize Indexes

```sql
-- Regular index maintenance
ALTER INDEX ALL ON Users REBUILD;
UPDATE STATISTICS Users WITH FULLSCAN;

-- Remove unused indexes
DROP INDEX IX_Users_UnusedColumn ON Users;
```

### 5. Monitor and Alert

```csharp
// Track slow queries
public class SlowQueryTracker
{
    private const int SlowQueryThresholdMs = 1000;

    public async Task<T> ExecuteWithTrackingAsync<T>(
        Func<Task<T>> query,
        string queryName)
    {
        var sw = Stopwatch.StartNew();
        var result = await query();
        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowQueryThresholdMs)
        {
            Logger.Warning($"Slow query '{queryName}': {sw.ElapsedMilliseconds}ms");
            // Send alert, log to monitoring system, etc.
        }

        return result;
    }
}
```

## Performance Checklist

### Query Level
- [ ] Use AsNoTracking for read-only queries
- [ ] Select only required columns
- [ ] Avoid N+1 queries with Include
- [ ] Use pagination for large result sets
- [ ] Use compiled queries for frequently-run queries
- [ ] Filter in database, not in memory

### Index Level
- [ ] Index all foreign keys
- [ ] Index columns used in WHERE clauses
- [ ] Use composite indexes for common query patterns
- [ ] Consider covering indexes for expensive queries
- [ ] Remove unused indexes
- [ ] Rebuild fragmented indexes regularly

### Caching Level
- [ ] Cache reference data
- [ ] Use distributed cache for web farms
- [ ] Invalidate cache on updates
- [ ] Set appropriate expiration times
- [ ] Monitor cache hit ratios

### Connection Level
- [ ] Use connection pooling
- [ ] Release connections quickly
- [ ] Configure appropriate pool sizes
- [ ] Use async/await consistently
- [ ] Dispose contexts properly

### Monitoring Level
- [ ] Enable query logging in development
- [ ] Track slow queries in production
- [ ] Monitor connection pool exhaustion
- [ ] Set up alerts for performance degradation
- [ ] Review query execution plans regularly

## Tools and Resources

### Profiling Tools
- **SQL Server Profiler**: Query analysis
- **SQL Server Management Studio**: Execution plans
- **Entity Framework Profiler**: EF Core query analysis
- **Azure Data Studio**: Query performance insights
- **MiniProfiler**: ASP.NET Core profiling

### Libraries
- **Dapper**: Micro-ORM for high performance
- **EFCore.BulkExtensions**: Bulk operations
- **Z.EntityFramework.Extensions**: Enterprise extensions
- **StackExchange.Redis**: Distributed caching
- **Polly**: Resilience and retry policies

### Further Reading
- [EF Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)
- [SQL Server Performance Tuning](https://learn.microsoft.com/en-us/sql/relational-databases/performance/performance-tuning)
- [Redis Best Practices](https://redis.io/docs/manual/patterns/)

## Related Documents

- [Schema Design](./schema-design.md)
- [ORM Patterns](./orm-patterns.md)
- [Migration Strategy](./migration-strategy.md)
- [Repository Pattern Examples](../../examples/data-access/repository-pattern/)
