# Layered Architecture for Windows Applications

This document defines the layered architecture approach for Windows applications, including dependency injection patterns, clean architecture principles, and SOLID implementation guidelines.

## Table of Contents

- [Overview](#overview)
- [Architecture Layers](#architecture-layers)
- [Dependency Injection](#dependency-injection)
- [Clean Architecture Guidelines](#clean-architecture-guidelines)
- [SOLID Principles](#solid-principles)
- [Design Patterns](#design-patterns)
- [Best Practices](#best-practices)

## Overview

Layered architecture organizes code into horizontal layers, each with a specific responsibility. This approach provides:

- **Separation of Concerns**: Each layer handles distinct responsibilities
- **Maintainability**: Changes in one layer minimize impact on others
- **Testability**: Layers can be tested independently
- **Scalability**: Layers can evolve independently
- **Team Collaboration**: Different teams can work on different layers

### Core Principles

1. **Dependency Rule**: Dependencies point inward toward business logic
2. **Abstraction**: Outer layers depend on inner layer interfaces, not implementations
3. **Isolation**: Each layer is loosely coupled and highly cohesive
4. **Testability**: Inner layers are testable without outer layer dependencies

## Architecture Layers

### Layer Overview

```
┌─────────────────────────────────────────────────────────┐
│                  Presentation Layer                      │
│              (UI, Controllers, ViewModels)               │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                 Application/Service Layer                │
│           (Use Cases, Application Services)              │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Business Logic Layer                  │
│            (Domain Models, Business Rules)               │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                   Data Access Layer                      │
│          (Repositories, Database Context)                │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                    │
│    (External Services, File I/O, Network, Logging)      │
└─────────────────────────────────────────────────────────┘
```

### 1. Presentation Layer

**Responsibility**: User interface and user interaction

**Components**:
- XAML Views (WPF/WinUI 3)
- View Models (MVVM pattern)
- UI Controls and Components
- Data Binding Logic
- Input Validation (UI-level)
- Navigation Logic

**Example Structure**:
```
/ProjectName.Presentation
├── /Views
│   ├── MainWindow.xaml
│   ├── SettingsPage.xaml
│   └── PackageListView.xaml
├── /ViewModels
│   ├── MainViewModel.cs
│   ├── SettingsViewModel.cs
│   └── PackageListViewModel.cs
├── /Controls
│   └── CustomButton.xaml
├── /Converters
│   └── BoolToVisibilityConverter.cs
└── /Services
    └── NavigationService.cs
```

**Code Example**:
```csharp
// ViewModel depends on application services
public class PackageListViewModel : ViewModelBase
{
    private readonly IPackageService _packageService;
    private readonly IDialogService _dialogService;
    
    public PackageListViewModel(
        IPackageService packageService,
        IDialogService dialogService)
    {
        _packageService = packageService;
        _dialogService = dialogService;
    }
    
    public async Task LoadPackagesAsync()
    {
        try
        {
            var packages = await _packageService.GetInstalledPackagesAsync();
            Packages = new ObservableCollection<PackageViewModel>(
                packages.Select(p => new PackageViewModel(p)));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Failed to load packages", ex.Message);
        }
    }
}
```

### 2. Application/Service Layer

**Responsibility**: Application use cases and orchestration

**Components**:
- Application Services
- Use Case Implementations
- Service Interfaces
- Application-Level Validation
- Transaction Management
- Security and Authorization

**Example Structure**:
```
/ProjectName.Application
├── /Services
│   ├── PackageService.cs
│   ├── SettingsService.cs
│   └── UpdateService.cs
├── /Interfaces
│   ├── IPackageService.cs
│   ├── ISettingsService.cs
│   └── IUpdateService.cs
├── /DTOs
│   ├── PackageDto.cs
│   └── SettingsDto.cs
└── /Validators
    └── PackageValidator.cs
```

**Code Example**:
```csharp
public interface IPackageService
{
    Task<IEnumerable<PackageDto>> GetInstalledPackagesAsync();
    Task<bool> InstallPackageAsync(string packageId);
    Task<bool> UninstallPackageAsync(string packageId);
    Task<IEnumerable<PackageDto>> SearchPackagesAsync(string query);
}

public class PackageService : IPackageService
{
    private readonly IPackageRepository _repository;
    private readonly IPackageManagerEngine _engine;
    private readonly ILogger<PackageService> _logger;
    
    public PackageService(
        IPackageRepository repository,
        IPackageManagerEngine engine,
        ILogger<PackageService> logger)
    {
        _repository = repository;
        _engine = engine;
        _logger = logger;
    }
    
    public async Task<IEnumerable<PackageDto>> GetInstalledPackagesAsync()
    {
        try
        {
            var packages = await _repository.GetInstalledAsync();
            return packages.Select(p => MapToDto(p));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve installed packages");
            throw new ApplicationException("Unable to load packages", ex);
        }
    }
    
    public async Task<bool> InstallPackageAsync(string packageId)
    {
        // Orchestrate the installation process
        var package = await _repository.FindByIdAsync(packageId);
        if (package == null)
            throw new NotFoundException($"Package {packageId} not found");
            
        var result = await _engine.InstallAsync(package);
        
        if (result.Success)
        {
            package.MarkAsInstalled();
            await _repository.UpdateAsync(package);
        }
        
        return result.Success;
    }
}
```

### 3. Business Logic Layer (Domain Layer)

**Responsibility**: Core business logic and domain models

**Components**:
- Domain Entities
- Value Objects
- Business Rules
- Domain Services
- Domain Events
- Specifications

**Example Structure**:
```
/ProjectName.Domain
├── /Entities
│   ├── Package.cs
│   ├── PackageVersion.cs
│   └── PackageSource.cs
├── /ValueObjects
│   ├── SemanticVersion.cs
│   └── PackageId.cs
├── /Interfaces
│   ├── IPackageRepository.cs
│   └── IPackageValidator.cs
├── /Services
│   └── PackageValidationService.cs
├── /Enums
│   └── PackageStatus.cs
└── /Exceptions
    └── InvalidPackageException.cs
```

**Code Example**:
```csharp
// Domain Entity
public class Package
{
    public PackageId Id { get; private set; }
    public string Name { get; private set; }
    public SemanticVersion Version { get; private set; }
    public PackageStatus Status { get; private set; }
    public DateTime InstalledDate { get; private set; }
    
    private Package() { } // For EF Core
    
    public Package(PackageId id, string name, SemanticVersion version)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Package name cannot be empty", nameof(name));
            
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name;
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Status = PackageStatus.Available;
    }
    
    // Business logic methods
    public void MarkAsInstalled()
    {
        if (Status == PackageStatus.Installed)
            throw new InvalidOperationException("Package is already installed");
            
        Status = PackageStatus.Installed;
        InstalledDate = DateTime.UtcNow;
    }
    
    public bool CanBeUpdated(SemanticVersion newVersion)
    {
        return Status == PackageStatus.Installed && 
               newVersion > Version;
    }
}

// Value Object
public record SemanticVersion : IComparable<SemanticVersion>
{
    public int Major { get; init; }
    public int Minor { get; init; }
    public int Patch { get; init; }
    
    public SemanticVersion(int major, int minor, int patch)
    {
        if (major < 0 || minor < 0 || patch < 0)
            throw new ArgumentException("Version components must be non-negative");
            
        Major = major;
        Minor = minor;
        Patch = patch;
    }
    
    public static SemanticVersion Parse(string version)
    {
        var parts = version.Split('.');
        if (parts.Length != 3)
            throw new FormatException("Invalid version format");
            
        return new SemanticVersion(
            int.Parse(parts[0]),
            int.Parse(parts[1]),
            int.Parse(parts[2])
        );
    }
    
    public int CompareTo(SemanticVersion? other)
    {
        if (other is null) return 1;
        
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;
        
        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;
        
        return Patch.CompareTo(other.Patch);
    }
    
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
```

### 4. Data Access Layer

**Responsibility**: Data persistence and retrieval

**Components**:
- Repository Implementations
- Database Context
- Data Models/Entities
- Migrations
- Query Objects

**Example Structure**:
```
/ProjectName.Data
├── /Repositories
│   ├── PackageRepository.cs
│   └── SettingsRepository.cs
├── /Context
│   └── ApplicationDbContext.cs
├── /Entities
│   └── PackageEntity.cs
├── /Configurations
│   └── PackageConfiguration.cs
└── /Migrations
    └── 001_InitialCreate.cs
```

**Code Example**:
```csharp
// Repository Interface (in Domain layer)
public interface IPackageRepository
{
    Task<Package?> FindByIdAsync(PackageId id);
    Task<IEnumerable<Package>> GetInstalledAsync();
    Task AddAsync(Package package);
    Task UpdateAsync(Package package);
    Task DeleteAsync(PackageId id);
}

// Repository Implementation (in Data layer)
public class PackageRepository : IPackageRepository
{
    private readonly ApplicationDbContext _context;
    
    public PackageRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Package?> FindByIdAsync(PackageId id)
    {
        var entity = await _context.Packages
            .FirstOrDefaultAsync(p => p.Id == id.Value);
            
        return entity?.ToDomainModel();
    }
    
    public async Task<IEnumerable<Package>> GetInstalledAsync()
    {
        var entities = await _context.Packages
            .Where(p => p.Status == PackageStatus.Installed)
            .ToListAsync();
            
        return entities.Select(e => e.ToDomainModel());
    }
    
    public async Task AddAsync(Package package)
    {
        var entity = PackageEntity.FromDomainModel(package);
        _context.Packages.Add(entity);
        await _context.SaveChangesAsync();
    }
}

// Database Context
public class ApplicationDbContext : DbContext
{
    public DbSet<PackageEntity> Packages { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new PackageConfiguration());
    }
}
```

### 5. Infrastructure Layer

**Responsibility**: External concerns and cross-cutting features

**Components**:
- External API Clients
- File System Operations
- Network Communications
- Logging Infrastructure
- Caching
- Email/SMS Services
- Message Queue Clients

**Example Structure**:
```
/ProjectName.Infrastructure
├── /ExternalServices
│   ├── WinGetClient.cs
│   └── GitHubApiClient.cs
├── /Logging
│   └── FileLogger.cs
├── /Caching
│   └── MemoryCacheService.cs
└── /IO
    └── FileStorageService.cs
```

**Code Example**:
```csharp
public interface IPackageManagerEngine
{
    Task<InstallationResult> InstallAsync(Package package);
    Task<bool> UninstallAsync(PackageId id);
}

public class WinGetEngine : IPackageManagerEngine
{
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<WinGetEngine> _logger;
    
    public WinGetEngine(
        IProcessRunner processRunner,
        ILogger<WinGetEngine> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
    }
    
    public async Task<InstallationResult> InstallAsync(Package package)
    {
        try
        {
            var arguments = $"install {package.Id} --silent";
            var result = await _processRunner.RunAsync("winget", arguments);
            
            return new InstallationResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                ErrorOutput = result.Error
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install package {PackageId}", package.Id);
            throw;
        }
    }
}
```

## Dependency Injection

### Why Dependency Injection?

- **Loose Coupling**: Components depend on abstractions, not concrete types
- **Testability**: Easy to mock dependencies in unit tests
- **Flexibility**: Swap implementations without changing dependent code
- **Lifetime Management**: Framework manages object creation and disposal

### Setting Up DI Container

**Program.cs / App.xaml.cs**:
```csharp
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    
    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddSingleton<ILogger, FileLogger>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        
        // Data Access
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=app.db"));
        services.AddScoped<IPackageRepository, PackageRepository>();
        
        // Business Logic
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<ISettingsService, SettingsService>();
        
        // External Services
        services.AddHttpClient<IPackageManagerEngine, WinGetEngine>();
        
        // Presentation
        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<IDialogService, DialogService>();
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
```

### Service Lifetimes

**Singleton**: One instance for the entire application lifetime
```csharp
services.AddSingleton<ILogger, FileLogger>();
```

**Scoped**: One instance per scope (per request in web apps, per window in desktop)
```csharp
services.AddScoped<IPackageService, PackageService>();
```

**Transient**: New instance every time it's requested
```csharp
services.AddTransient<MainViewModel>();
```

### Constructor Injection

```csharp
public class PackageService : IPackageService
{
    private readonly IPackageRepository _repository;
    private readonly IPackageManagerEngine _engine;
    private readonly ILogger<PackageService> _logger;
    
    // Dependencies injected via constructor
    public PackageService(
        IPackageRepository repository,
        IPackageManagerEngine engine,
        ILogger<PackageService> logger)
    {
        _repository = repository;
        _engine = engine;
        _logger = logger;
    }
}
```

## Clean Architecture Guidelines

Clean Architecture (also known as Hexagonal Architecture or Ports and Adapters) emphasizes:

1. **Independence from Frameworks**: Business logic doesn't depend on external frameworks
2. **Testability**: Business logic can be tested without UI, database, or external services
3. **Independence from UI**: UI can change without affecting business logic
4. **Independence from Database**: Can swap databases without changing business rules
5. **Independence from External Services**: Business logic doesn't know about external systems

### Dependency Flow

```
┌─────────────────────────────────────────┐
│         UI / External Services          │ ← Outer Layer
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│        Application Services             │ ← Application Layer
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│          Business Logic                 │ ← Inner Layer (Core)
└─────────────────────────────────────────┘
```

**Key Rule**: Source code dependencies point only inward, toward the business logic.

### Implementing Clean Architecture

**1. Define Core Domain (Inner Layer)**
```csharp
// ProjectName.Domain/Entities/Package.cs
public class Package
{
    // No dependencies on outer layers
    // Pure business logic
}
```

**2. Define Application Interfaces (Application Layer)**
```csharp
// ProjectName.Application/Interfaces/IPackageRepository.cs
public interface IPackageRepository
{
    // Interface in application layer
    // Implementation in infrastructure layer
}
```

**3. Implement Infrastructure (Outer Layer)**
```csharp
// ProjectName.Infrastructure/Repositories/PackageRepository.cs
public class PackageRepository : IPackageRepository
{
    // Implementation depends on interface (inner layer)
    // Uses EF Core, but domain doesn't know about it
}
```

**4. Inversion of Control**
```csharp
// Dependency injection wires everything together
services.AddScoped<IPackageRepository, PackageRepository>();
```

## SOLID Principles

### Single Responsibility Principle (SRP)

**Definition**: A class should have only one reason to change.

**Example**:
```csharp
// BAD: Multiple responsibilities
public class PackageManager
{
    public void InstallPackage(string id) { }
    public void LogInstallation(string message) { }
    public void SendEmail(string to, string subject) { }
}

// GOOD: Single responsibility per class
public class PackageService
{
    private readonly ILogger _logger;
    private readonly IEmailService _emailService;
    
    public async Task InstallPackageAsync(string id)
    {
        // Only handles package installation
        await DoInstallAsync(id);
        _logger.LogInformation($"Installed {id}");
        await _emailService.SendAsync("admin@example.com", $"Package {id} installed");
    }
}

public class Logger : ILogger
{
    // Only handles logging
}

public class EmailService : IEmailService
{
    // Only handles email
}
```

### Open/Closed Principle (OCP)

**Definition**: Software entities should be open for extension but closed for modification.

**Example**:
```csharp
// BAD: Requires modification to add new package managers
public class PackageInstaller
{
    public void Install(string manager, string packageId)
    {
        if (manager == "winget")
            InstallWithWinGet(packageId);
        else if (manager == "scoop")
            InstallWithScoop(packageId);
        // Must modify this method to add new managers
    }
}

// GOOD: Extensible through polymorphism
public interface IPackageManager
{
    Task InstallAsync(string packageId);
}

public class WinGetManager : IPackageManager
{
    public async Task InstallAsync(string packageId)
    {
        // WinGet-specific implementation
    }
}

public class ScoopManager : IPackageManager
{
    public async Task InstallAsync(string packageId)
    {
        // Scoop-specific implementation
    }
}

public class PackageInstaller
{
    private readonly IEnumerable<IPackageManager> _managers;
    
    public PackageInstaller(IEnumerable<IPackageManager> managers)
    {
        _managers = managers;
        // Can add new managers without changing this code
    }
}
```

### Liskov Substitution Principle (LSP)

**Definition**: Derived classes must be substitutable for their base classes.

**Example**:
```csharp
// BAD: Square violates LSP because it changes Rectangle behavior
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
}

public class Square : Rectangle
{
    private int _side;
    
    public override int Width
    {
        get => _side;
        set => _side = value; // Sets both dimensions
    }
    
    public override int Height
    {
        get => _side;
        set => _side = value; // Violates expected Rectangle behavior
    }
}

// GOOD: Use composition instead
public interface IShape
{
    int CalculateArea();
}

public class Rectangle : IShape
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    public int CalculateArea() => Width * Height;
}

public class Square : IShape
{
    public int Side { get; set; }
    
    public int CalculateArea() => Side * Side;
}
```

### Interface Segregation Principle (ISP)

**Definition**: Clients should not be forced to depend on interfaces they don't use.

**Example**:
```csharp
// BAD: Fat interface with many methods
public interface IPackageManager
{
    Task InstallAsync(string id);
    Task UninstallAsync(string id);
    Task SearchAsync(string query);
    Task GetDetailsAsync(string id);
    Task AddSourceAsync(string source);
    Task RemoveSourceAsync(string source);
    Task ListSourcesAsync();
}

// GOOD: Segregated interfaces
public interface IPackageInstaller
{
    Task InstallAsync(string id);
    Task UninstallAsync(string id);
}

public interface IPackageSearcher
{
    Task<IEnumerable<Package>> SearchAsync(string query);
    Task<Package> GetDetailsAsync(string id);
}

public interface ISourceManager
{
    Task AddSourceAsync(string source);
    Task RemoveSourceAsync(string source);
    Task<IEnumerable<string>> ListSourcesAsync();
}

// Implement only what's needed
public class WinGetManager : IPackageInstaller, IPackageSearcher
{
    // Implements only installation and search
    // Not forced to implement source management if not supported
}
```

### Dependency Inversion Principle (DIP)

**Definition**: High-level modules should not depend on low-level modules. Both should depend on abstractions.

**Example**:
```csharp
// BAD: High-level depends on low-level
public class PackageService
{
    private readonly SqlPackageRepository _repository; // Concrete dependency
    
    public PackageService()
    {
        _repository = new SqlPackageRepository(); // Tight coupling
    }
}

// GOOD: Both depend on abstraction
public interface IPackageRepository // Abstraction
{
    Task<Package> GetByIdAsync(string id);
}

public class PackageService // High-level module
{
    private readonly IPackageRepository _repository;
    
    public PackageService(IPackageRepository repository)
    {
        _repository = repository; // Depends on abstraction
    }
}

public class SqlPackageRepository : IPackageRepository // Low-level module
{
    // Implementation details
    public async Task<Package> GetByIdAsync(string id)
    {
        // SQL-specific code
    }
}
```

## Design Patterns

### Repository Pattern

Abstracts data access logic:

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    // ... other methods
}
```

### Unit of Work Pattern

Manages transactions across multiple repositories:

```csharp
public interface IUnitOfWork : IDisposable
{
    IPackageRepository Packages { get; }
    ISettingsRepository Settings { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public IPackageRepository Packages { get; }
    public ISettingsRepository Settings { get; }
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Packages = new PackageRepository(context);
        Settings = new SettingsRepository(context);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Factory Pattern

Creates objects without specifying exact class:

```csharp
public interface IPackageManagerFactory
{
    IPackageManager Create(string managerType);
}

public class PackageManagerFactory : IPackageManagerFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public PackageManagerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IPackageManager Create(string managerType)
    {
        return managerType switch
        {
            "winget" => _serviceProvider.GetRequiredService<WinGetManager>(),
            "scoop" => _serviceProvider.GetRequiredService<ScoopManager>(),
            "chocolatey" => _serviceProvider.GetRequiredService<ChocolateyManager>(),
            _ => throw new ArgumentException($"Unknown manager type: {managerType}")
        };
    }
}
```

### Strategy Pattern

Defines family of interchangeable algorithms:

```csharp
public interface IInstallationStrategy
{
    Task<bool> InstallAsync(Package package);
}

public class SilentInstallStrategy : IInstallationStrategy
{
    public async Task<bool> InstallAsync(Package package)
    {
        // Silent installation logic
    }
}

public class InteractiveInstallStrategy : IInstallationStrategy
{
    public async Task<bool> InstallAsync(Package package)
    {
        // Interactive installation logic
    }
}

public class PackageInstaller
{
    private IInstallationStrategy _strategy;
    
    public void SetStrategy(IInstallationStrategy strategy)
    {
        _strategy = strategy;
    }
    
    public async Task<bool> InstallPackageAsync(Package package)
    {
        return await _strategy.InstallAsync(package);
    }
}
```

### Observer Pattern

Allows objects to notify observers of state changes:

```csharp
public interface IPackageInstallationObserver
{
    void OnInstallationStarted(string packageId);
    void OnInstallationProgress(string packageId, int progress);
    void OnInstallationCompleted(string packageId, bool success);
}

public class PackageInstaller
{
    private readonly List<IPackageInstallationObserver> _observers = new();
    
    public void Attach(IPackageInstallationObserver observer)
    {
        _observers.Add(observer);
    }
    
    public void Detach(IPackageInstallationObserver observer)
    {
        _observers.Remove(observer);
    }
    
    private void NotifyInstallationStarted(string packageId)
    {
        foreach (var observer in _observers)
        {
            observer.OnInstallationStarted(packageId);
        }
    }
    
    public async Task InstallAsync(string packageId)
    {
        NotifyInstallationStarted(packageId);
        // ... installation logic
    }
}
```

## Best Practices

### 1. Keep Business Logic Independent

```csharp
// GOOD: Domain logic has no external dependencies
public class Order
{
    public bool CanBeShipped()
    {
        return Status == OrderStatus.Paid && Items.Any();
    }
}

// BAD: Domain logic depends on infrastructure
public class Order
{
    public bool CanBeShipped(IDatabase db)
    {
        var status = db.Query("SELECT Status FROM Orders WHERE Id = @Id", Id);
        return status == "Paid";
    }
}
```

### 2. Use Interfaces for Abstraction

```csharp
// Define interfaces in the layer that needs them
// Implement in outer layers

// Domain/Application Layer
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

// Infrastructure Layer
public class SmtpEmailService : IEmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        // SMTP implementation
    }
}
```

### 3. Avoid Anemic Domain Models

```csharp
// BAD: Anemic model (just data, no behavior)
public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
}

// GOOD: Rich domain model (data + behavior)
public class Order
{
    public int Id { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    
    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot add items to non-draft order");
            
        _items.Add(item);
        RecalculateTotal();
    }
    
    public void Submit()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Cannot submit empty order");
            
        Status = OrderStatus.Submitted;
    }
    
    private void RecalculateTotal()
    {
        Total = _items.Sum(i => i.Price * i.Quantity);
    }
}
```

### 4. Keep Controllers/ViewModels Thin

```csharp
// GOOD: ViewModel delegates to services
public class PackageListViewModel
{
    private readonly IPackageService _packageService;
    
    public async Task LoadPackagesAsync()
    {
        // Minimal logic, delegates to service
        Packages = await _packageService.GetInstalledPackagesAsync();
    }
}

// BAD: ViewModel contains business logic
public class PackageListViewModel
{
    public async Task LoadPackagesAsync()
    {
        // Too much logic in ViewModel
        var process = Process.Start("winget", "list");
        var output = await process.StandardOutput.ReadToEndAsync();
        var packages = ParseWinGetOutput(output);
        Packages = packages.Where(p => p.IsInstalled).ToList();
    }
}
```

### 5. Use DTOs for Layer Communication

```csharp
// DTO for transferring data between layers
public record PackageDto(
    string Id,
    string Name,
    string Version,
    bool IsInstalled
);

// Map between domain model and DTO
public static class PackageMapper
{
    public static PackageDto ToDto(Package package)
    {
        return new PackageDto(
            package.Id.Value,
            package.Name,
            package.Version.ToString(),
            package.Status == PackageStatus.Installed
        );
    }
}
```

### 6. Handle Cross-Cutting Concerns Appropriately

```csharp
// Use middleware/filters for cross-cutting concerns
public class LoggingService : IPackageService
{
    private readonly IPackageService _inner;
    private readonly ILogger _logger;
    
    public LoggingService(IPackageService inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }
    
    public async Task<bool> InstallPackageAsync(string packageId)
    {
        _logger.LogInformation($"Installing package {packageId}");
        try
        {
            var result = await _inner.InstallPackageAsync(packageId);
            _logger.LogInformation($"Package {packageId} installed: {result}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to install package {packageId}");
            throw;
        }
    }
}
```

## Testing Strategies

### Unit Testing

Test business logic in isolation:

```csharp
public class PackageServiceTests
{
    [Fact]
    public async Task InstallPackageAsync_ValidPackage_ReturnsTrue()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockEngine = new Mock<IPackageManagerEngine>();
        mockEngine.Setup(e => e.InstallAsync(It.IsAny<Package>()))
            .ReturnsAsync(new InstallationResult { Success = true });
            
        var service = new PackageService(
            mockRepository.Object,
            mockEngine.Object,
            Mock.Of<ILogger<PackageService>>()
        );
        
        // Act
        var result = await service.InstallPackageAsync("test-package");
        
        // Assert
        Assert.True(result);
    }
}
```

### Integration Testing

Test layer interactions:

```csharp
public class PackageRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PackageRepository _repository;
    
    public PackageRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        _repository = new PackageRepository(_context);
    }
    
    [Fact]
    public async Task AddAsync_ValidPackage_SavesToDatabase()
    {
        // Arrange
        var package = new Package(
            new PackageId("test-package"),
            "Test Package",
            SemanticVersion.Parse("1.0.0")
        );
        
        // Act
        await _repository.AddAsync(package);
        var retrieved = await _repository.FindByIdAsync(package.Id);
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(package.Name, retrieved.Name);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

## Related Documentation

- [Project Structure](./project-structure.md)
- [ADR Template](./adr-template.md)
- [Design Patterns](./design-patterns.md)

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Microsoft .NET Application Architecture Guides](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
