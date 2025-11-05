# Data Access Example

Demonstrates data access layer patterns for package management data.

## Patterns Covered

- **Repository Pattern** - Abstract data access
- **Unit of Work Pattern** - Transaction management
- **Entity Framework Core** - ORM implementation
- **Query Optimization** - Performance best practices
- **Caching Strategies** - Multi-level caching
- **Migration Management** - Database versioning

## Database Schema

```sql
CREATE TABLE Packages (
    Id NVARCHAR(255) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Version NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    Source NVARCHAR(50) NOT NULL,
    Publisher NVARCHAR(255),
    DownloadUrl NVARCHAR(500),
    Size BIGINT,
    IsInstalled BIT DEFAULT 0,
    InstalledDate DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE PackageSources (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    IsEnabled BIT DEFAULT 1,
    Priority INT DEFAULT 0
);

CREATE TABLE InstallationHistory (
    Id INT PRIMARY KEY IDENTITY,
    PackageId NVARCHAR(255) NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- Install, Uninstall, Update
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX),
    FOREIGN KEY (PackageId) REFERENCES Packages(Id)
);

CREATE INDEX IX_Packages_Source ON Packages(Source);
CREATE INDEX IX_Packages_IsInstalled ON Packages(IsInstalled);
CREATE INDEX IX_InstallationHistory_PackageId ON InstallationHistory(PackageId);
CREATE INDEX IX_InstallationHistory_Timestamp ON InstallationHistory(Timestamp DESC);
```

## Entity Models

```csharp
public class Package
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? DownloadUrl { get; set; }
    public long? Size { get; set; }
    public bool IsInstalled { get; set; }
    public DateTime? InstalledDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public virtual ICollection<InstallationHistory> History { get; set; } = new List<InstallationHistory>();
}

public class PackageSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
}

public class InstallationHistory
{
    public int Id { get; set; }
    public string PackageId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    public virtual Package? Package { get; set; }
}
```

## DbContext

```csharp
public class PackageDbContext : DbContext
{
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageSource> PackageSources { get; set; }
    public DbSet<InstallationHistory> InstallationHistory { get; set; }
    
    public PackageDbContext(DbContextOptions<PackageDbContext> options) 
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.IsInstalled);
        });
        
        modelBuilder.Entity<InstallationHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.Package)
                .WithMany(p => p.History)
                .HasForeignKey(e => e.PackageId);
            
            entity.HasIndex(e => e.PackageId);
            entity.HasIndex(e => e.Timestamp).IsDescending();
        });
    }
}
```

## Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly PackageDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(PackageDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
    
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }
    
    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }
    
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }
    
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}
```

## Specialized Repositories

```csharp
public interface IPackageRepository : IRepository<Package>
{
    Task<IEnumerable<Package>> GetInstalledPackagesAsync();
    Task<IEnumerable<Package>> GetPackagesBySourceAsync(string source);
    Task<IEnumerable<Package>> SearchPackagesAsync(string query);
    Task<Package?> GetPackageWithHistoryAsync(string packageId);
}

public class PackageRepository : Repository<Package>, IPackageRepository
{
    public PackageRepository(PackageDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Package>> GetInstalledPackagesAsync()
    {
        return await _dbSet
            .Where(p => p.IsInstalled)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Package>> GetPackagesBySourceAsync(string source)
    {
        return await _dbSet
            .Where(p => p.Source == source)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Package>> SearchPackagesAsync(string query)
    {
        return await _dbSet
            .Where(p => EF.Functions.Like(p.Name, $"%{query}%") 
                     || EF.Functions.Like(p.Description!, $"%{query}%"))
            .OrderBy(p => p.Name)
            .Take(100)
            .ToListAsync();
    }
    
    public async Task<Package?> GetPackageWithHistoryAsync(string packageId)
    {
        return await _dbSet
            .Include(p => p.History.OrderByDescending(h => h.Timestamp))
            .FirstOrDefaultAsync(p => p.Id == packageId);
    }
}
```

## Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    IPackageRepository Packages { get; }
    IRepository<PackageSource> PackageSources { get; }
    IRepository<InstallationHistory> InstallationHistory { get; }
    
    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly PackageDbContext _context;
    private IDbContextTransaction? _transaction;
    
    public IPackageRepository Packages { get; }
    public IRepository<PackageSource> PackageSources { get; }
    public IRepository<InstallationHistory> InstallationHistory { get; }
    
    public UnitOfWork(PackageDbContext context)
    {
        _context = context;
        Packages = new PackageRepository(context);
        PackageSources = new Repository<PackageSource>(context);
        InstallationHistory = new Repository<InstallationHistory>(context);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        // Update timestamps
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);
        
        foreach (var entry in entries)
        {
            if (entry.Entity is Package package)
            {
                package.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        return await _context.SaveChangesAsync();
    }
    
    public async Task<bool> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction != null;
    }
    
    public async Task<bool> CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
            return true;
        }
        return false;
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

## Service Layer with Caching

```csharp
public class PackageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PackageService> _logger;
    
    public PackageService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<PackageService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Package>> GetInstalledPackagesAsync()
    {
        const string cacheKey = "installed_packages";
        
        if (_cache.TryGetValue<IEnumerable<Package>>(cacheKey, out var cached))
        {
            return cached;
        }
        
        var packages = await _unitOfWork.Packages.GetInstalledPackagesAsync();
        _cache.Set(cacheKey, packages, TimeSpan.FromMinutes(5));
        
        return packages;
    }
    
    public async Task<bool> InstallPackageAsync(Package package)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            package.IsInstalled = true;
            package.InstalledDate = DateTime.UtcNow;
            
            await _unitOfWork.Packages.AddAsync(package);
            
            var history = new InstallationHistory
            {
                PackageId = package.Id,
                Action = "Install",
                Success = true
            };
            await _unitOfWork.InstallationHistory.AddAsync(history);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            // Invalidate cache
            _cache.Remove("installed_packages");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install package {PackageId}", package.Id);
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }
}
```

## Query Optimization Examples

```csharp
// Bad: N+1 query problem
public async Task<List<Package>> GetPackagesWithHistoryBad()
{
    var packages = await _context.Packages.ToListAsync();
    foreach (var package in packages)
    {
        // This executes a separate query for each package!
        var history = await _context.InstallationHistory
            .Where(h => h.PackageId == package.Id)
            .ToListAsync();
    }
    return packages;
}

// Good: Single query with eager loading
public async Task<List<Package>> GetPackagesWithHistoryGood()
{
    return await _context.Packages
        .Include(p => p.History)
        .ToListAsync();
}

// Good: Projection to reduce data transfer
public async Task<List<PackageDto>> GetPackageSummaries()
{
    return await _context.Packages
        .Select(p => new PackageDto
        {
            Id = p.Id,
            Name = p.Name,
            Version = p.Version
        })
        .ToListAsync();
}

// Good: AsNoTracking for read-only queries
public async Task<IEnumerable<Package>> SearchPackagesReadOnly(string query)
{
    return await _context.Packages
        .AsNoTracking()
        .Where(p => p.Name.Contains(query))
        .ToListAsync();
}
```

## Testing

```csharp
public class PackageRepositoryTests
{
    [Fact]
    public async Task GetInstalledPackages_ReturnsOnlyInstalled()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PackageDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        
        using var context = new PackageDbContext(options);
        var repository = new PackageRepository(context);
        
        await context.Packages.AddRangeAsync(
            new Package { Id = "1", Name = "Test1", IsInstalled = true },
            new Package { Id = "2", Name = "Test2", IsInstalled = false }
        );
        await context.SaveChangesAsync();
        
        // Act
        var installed = await repository.GetInstalledPackagesAsync();
        
        // Assert
        Assert.Single(installed);
        Assert.Equal("Test1", installed.First().Name);
    }
}
```

## Running the Example

```bash
cd data-access-example

# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Run application
dotnet run
```

## Best Practices

1. **Use Repository Pattern** for abstraction
2. **Implement Unit of Work** for transactions
3. **Enable Query Splitting** for complex includes
4. **Use AsNoTracking** for read-only queries
5. **Implement Caching** at appropriate levels
6. **Index frequently queried columns**
7. **Use Projections** to reduce data transfer
8. **Batch operations** when possible

## License

Part of the UniGetUI project examples.
