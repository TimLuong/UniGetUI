# Integration Testing Guide

## Overview

Integration tests verify that multiple components work together correctly. Unlike unit tests that isolate individual components, integration tests validate the interactions between different parts of the system, external dependencies, and infrastructure.

This guide covers integration testing strategies specifically for Windows applications built with .NET, WinUI 3, and multiple package manager integrations.

## What are Integration Tests?

Integration tests sit between unit tests and end-to-end tests in the testing pyramid:

```
        /\
       /  \  E2E Tests (Few)
      /____\
     /      \
    /        \ Integration Tests (Some)
   /__________\
  /            \
 /              \ Unit Tests (Many)
/________________\
```

### Characteristics

- **Scope**: Test multiple components working together
- **Speed**: Slower than unit tests, faster than E2E tests
- **Dependencies**: May use real databases, file systems, or external services
- **Isolation**: Tests should still be independent of each other
- **Reliability**: More prone to flakiness than unit tests but more reliable than E2E

## Integration Testing Patterns

### 1. Component Integration Testing

Test interactions between application components without external dependencies.

```csharp
public class PackageManagerIntegrationTests
{
    [Fact]
    public async Task PackageLoader_WithWinGetManager_LoadsPackages()
    {
        // Arrange
        var manager = new WinGetPackageManager();
        var loader = new PackageLoader(manager);
        
        // Act
        var packages = await loader.LoadPackagesAsync();
        
        // Assert
        Assert.NotEmpty(packages);
        Assert.All(packages, pkg => Assert.NotNull(pkg.Id));
    }
}
```

### 2. Database Integration Testing

Test data access layer with a real or in-memory database.

```csharp
public class PackageDatabaseIntegrationTests : IDisposable
{
    private readonly DatabaseContext _context;

    public PackageDatabaseIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new DatabaseContext(options);
    }

    [Fact]
    public async Task SavePackage_PersistsToDatabase()
    {
        // Arrange
        var repository = new PackageRepository(_context);
        var package = new Package { Id = "test.package", Name = "Test Package" };
        
        // Act
        await repository.SaveAsync(package);
        var retrieved = await repository.GetByIdAsync("test.package");
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Test Package", retrieved.Name);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### 3. External Service Integration Testing

Test integration with external services (with caution in CI/CD).

```csharp
public class WinGetServiceIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("ExternalDependency", "WinGet")]
    public async Task WinGet_SearchPackages_ReturnsResults()
    {
        // Skip if WinGet is not available
        var (available, _) = await CoreTools.WhichAsync("winget.exe");
        if (!available)
        {
            throw new SkipException("WinGet is not installed");
        }
        
        // Arrange
        var manager = new WinGetPackageManager();
        
        // Act
        var results = await manager.SearchPackagesAsync("vscode");
        
        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, pkg => pkg.Id.Contains("Microsoft.VisualStudioCode"));
    }
}
```

### 4. File System Integration Testing

Test file operations with temporary directories.

```csharp
public class FileSystemIntegrationTests : IDisposable
{
    private readonly string _testDirectory;

    public FileSystemIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task ExportPackageList_CreatesJsonFile()
    {
        // Arrange
        var exporter = new PackageExporter();
        var packages = new List<Package>
        {
            new() { Id = "pkg1", Name = "Package 1" },
            new() { Id = "pkg2", Name = "Package 2" }
        };
        var outputPath = Path.Combine(_testDirectory, "packages.json");
        
        // Act
        await exporter.ExportAsync(packages, outputPath);
        
        // Assert
        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("pkg1", content);
        Assert.Contains("pkg2", content);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
```

### 5. Process Integration Testing

Test external process execution and command-line tools.

```csharp
public class ProcessIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExecuteCommand_WithValidCommand_ReturnsOutput()
    {
        // Arrange
        var executor = new CommandExecutor();
        
        // Act
        var result = await executor.ExecuteAsync("cmd.exe", "/c echo Hello");
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains("Hello", result.Output);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task ExecuteCommand_WithInvalidCommand_ReturnsError()
    {
        // Arrange
        var executor = new CommandExecutor();
        
        // Act
        var result = await executor.ExecuteAsync("nonexistent.exe", "");
        
        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Error);
    }
}
```

## Test Data Management

### 1. Test Data Builders

Use the Builder pattern for creating complex test data:

```csharp
public class PackageBuilder
{
    private string _id = "default.package";
    private string _name = "Default Package";
    private string _version = "1.0.0";
    private string _source = "WinGet";

    public PackageBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public PackageBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PackageBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    public PackageBuilder WithSource(string source)
    {
        _source = source;
        return this;
    }

    public Package Build()
    {
        return new Package
        {
            Id = _id,
            Name = _name,
            Version = _version,
            Source = _source
        };
    }
}

// Usage in tests
[Fact]
public void TestWithBuilder()
{
    var package = new PackageBuilder()
        .WithId("test.package")
        .WithName("Test Package")
        .WithVersion("2.0.0")
        .Build();
    
    // Use package in test
}
```

### 2. Object Mother Pattern

Create common test objects through factory methods:

```csharp
public static class TestPackages
{
    public static Package CreateDefault()
    {
        return new Package
        {
            Id = "default.package",
            Name = "Default Package",
            Version = "1.0.0"
        };
    }

    public static Package CreateWithCustomOptions()
    {
        return new Package
        {
            Id = "custom.package",
            Name = "Custom Package",
            Version = "2.0.0",
            InstallationOptions = new InstallOptions
            {
                RunAsAdministrator = true,
                SkipHashCheck = true
            }
        };
    }

    public static List<Package> CreatePackageList(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Package
            {
                Id = $"package{i}",
                Name = $"Package {i}",
                Version = "1.0.0"
            })
            .ToList();
    }
}

// Usage
[Fact]
public void TestWithObjectMother()
{
    var package = TestPackages.CreateDefault();
    var packages = TestPackages.CreatePackageList(10);
}
```

### 3. Test Fixtures for Shared Data

```csharp
public class DatabaseFixture : IDisposable
{
    public DatabaseContext Context { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        Context = new DatabaseContext(options);
        
        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        Context.Packages.AddRange(
            new Package { Id = "pkg1", Name = "Package 1" },
            new Package { Id = "pkg2", Name = "Package 2" },
            new Package { Id = "pkg3", Name = "Package 3" }
        );
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database collection")]
public class PackageQueryTests
{
    private readonly DatabaseFixture _fixture;

    public PackageQueryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetAllPackages_ReturnsSeededData()
    {
        var packages = _fixture.Context.Packages.ToList();
        Assert.Equal(3, packages.Count);
    }
}
```

## Mocking and Stubbing Strategies

### When to Mock

- **External services**: APIs, databases, file systems
- **Slow operations**: Network calls, large file processing
- **Non-deterministic behavior**: Random number generators, current time
- **Side effects**: Email sending, logging, notifications

### When NOT to Mock

- **Simple objects**: POCOs, DTOs, value objects
- **The system under test**: Never mock what you're testing
- **Stable, fast dependencies**: Well-tested internal libraries

### Using Moq

Moq is the recommended mocking library for .NET:

```csharp
// Install Moq
// dotnet add package Moq

using Moq;

public class PackageServiceIntegrationTests
{
    [Fact]
    public async Task InstallPackage_WithMockedManager_CallsCorrectMethods()
    {
        // Arrange
        var mockManager = new Mock<IPackageManager>();
        mockManager
            .Setup(m => m.InstallPackageAsync(It.IsAny<Package>()))
            .ReturnsAsync(true);
        
        var service = new PackageService(mockManager.Object);
        var package = new Package { Id = "test.package" };
        
        // Act
        var result = await service.InstallAsync(package);
        
        // Assert
        Assert.True(result);
        mockManager.Verify(m => m.InstallPackageAsync(package), Times.Once);
    }
}
```

### Mock Setup Patterns

```csharp
// Return specific value
mock.Setup(m => m.GetValue()).Returns(42);

// Return async result
mock.Setup(m => m.GetValueAsync()).ReturnsAsync(42);

// Return different values on successive calls
mock.SetupSequence(m => m.GetValue())
    .Returns(1)
    .Returns(2)
    .Returns(3);

// Throw exception
mock.Setup(m => m.GetValue()).Throws<InvalidOperationException>();

// Match specific arguments
mock.Setup(m => m.GetById(123)).Returns(someValue);

// Match any arguments
mock.Setup(m => m.GetById(It.IsAny<int>())).Returns(someValue);

// Match with predicate
mock.Setup(m => m.GetById(It.Is<int>(id => id > 0))).Returns(someValue);

// Callback
mock.Setup(m => m.Save(It.IsAny<Data>()))
    .Callback<Data>(data => Console.WriteLine($"Saving {data.Id}"))
    .Returns(true);
```

### Using NSubstitute

NSubstitute is an alternative with a more fluent syntax:

```csharp
// Install NSubstitute
// dotnet add package NSubstitute

using NSubstitute;

public class PackageServiceTests
{
    [Fact]
    public async Task InstallPackage_WithSubstitute_Works()
    {
        // Arrange
        var manager = Substitute.For<IPackageManager>();
        manager.InstallPackageAsync(Arg.Any<Package>()).Returns(true);
        
        var service = new PackageService(manager);
        var package = new Package { Id = "test.package" };
        
        // Act
        var result = await service.InstallAsync(package);
        
        // Assert
        Assert.True(result);
        await manager.Received(1).InstallPackageAsync(package);
    }
}
```

### Stub Pattern

For simple test doubles without verification:

```csharp
public class StubPackageManager : IPackageManager
{
    private readonly List<Package> _packages;

    public StubPackageManager(List<Package> packages)
    {
        _packages = packages;
    }

    public Task<List<Package>> GetPackagesAsync()
    {
        return Task.FromResult(_packages);
    }

    public Task<bool> InstallPackageAsync(Package package)
    {
        return Task.FromResult(true);
    }
}

[Fact]
public async Task TestWithStub()
{
    var packages = new List<Package>
    {
        new() { Id = "pkg1", Name = "Package 1" }
    };
    
    var stub = new StubPackageManager(packages);
    var service = new PackageService(stub);
    
    var result = await service.GetAllPackagesAsync();
    Assert.Single(result);
}
```

## Testing Async Code

### Async/Await Best Practices

```csharp
// ✅ Good - Proper async test
[Fact]
public async Task AsyncMethod_ReturnsExpectedResult()
{
    var result = await service.GetDataAsync();
    Assert.NotNull(result);
}

// ❌ Bad - Blocking async call
[Fact]
public void AsyncMethod_ReturnsExpectedResult()
{
    var result = service.GetDataAsync().Result; // Deadlock risk!
    Assert.NotNull(result);
}

// ❌ Bad - Not awaiting
[Fact]
public async Task AsyncMethod_ReturnsExpectedResult()
{
    service.GetDataAsync(); // Fire and forget - test completes before method
    // Test might pass even if method throws!
}
```

### Testing Parallel Operations

```csharp
[Fact]
public async Task ParallelOperations_CompleteSuccessfully()
{
    // Arrange
    var manager = new PackageManager();
    var packages = CreateTestPackages(10);
    
    // Act
    var tasks = packages.Select(pkg => manager.InstallAsync(pkg));
    var results = await Task.WhenAll(tasks);
    
    // Assert
    Assert.All(results, result => Assert.True(result));
}
```

### Testing Timeout Behavior

```csharp
[Fact]
public async Task SlowOperation_TimesOutCorrectly()
{
    // Arrange
    var service = new TimeoutService(timeout: TimeSpan.FromSeconds(1));
    
    // Act & Assert
    await Assert.ThrowsAsync<TimeoutException>(async () =>
    {
        await service.VerySlowOperationAsync();
    });
}
```

## Testing External Dependencies

### Package Manager Integration

```csharp
public class WinGetIntegrationTests
{
    private bool IsWinGetAvailable()
    {
        var (available, _) = CoreTools.WhichAsync("winget.exe").Result;
        return available;
    }

    [Fact]
    public async Task WinGet_ListPackages_ReturnsResults()
    {
        // Skip test if WinGet not available
        if (!IsWinGetAvailable())
        {
            throw new SkipException("WinGet is not installed on this system");
        }
        
        // Arrange
        var manager = new WinGetPackageManager();
        
        // Act
        var packages = await manager.ListInstalledPackagesAsync();
        
        // Assert
        Assert.NotNull(packages);
        // Don't assert exact count - varies by system
        Assert.IsType<List<Package>>(packages);
    }
}
```

### Network Service Integration

```csharp
public class ApiIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Requires", "Network")]
    public async Task FetchPackageInfo_WithValidId_ReturnsData()
    {
        // Arrange
        var client = new PackageApiClient("https://api.example.com");
        
        // Act
        var packageInfo = await client.GetPackageInfoAsync("valid.package");
        
        // Assert
        Assert.NotNull(packageInfo);
        Assert.Equal("valid.package", packageInfo.Id);
    }

    [Fact]
    public async Task FetchPackageInfo_WithMockedHttp_ReturnsData()
    {
        // Use HttpClient mocking for reliable tests
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://api.example.com/*")
            .Respond("application/json", "{ 'id': 'valid.package', 'name': 'Package' }");
        
        var client = new PackageApiClient(new HttpClient(mockHandler));
        
        var packageInfo = await client.GetPackageInfoAsync("valid.package");
        
        Assert.NotNull(packageInfo);
    }
}
```

## Test Organization and Categories

### Categorizing Tests

Use traits to organize integration tests:

```csharp
[Trait("Category", "Integration")]
[Trait("Layer", "DataAccess")]
public class DatabaseIntegrationTests { }

[Trait("Category", "Integration")]
[Trait("Layer", "Service")]
[Trait("ExternalDependency", "WinGet")]
public class WinGetServiceTests { }

[Trait("Category", "Integration")]
[Trait("Speed", "Slow")]
public class SlowIntegrationTests { }
```

### Running Specific Test Categories

```bash
# Run all integration tests
dotnet test --filter "Category=Integration"

# Run only fast integration tests
dotnet test --filter "Category=Integration&Speed!=Slow"

# Run data access tests
dotnet test --filter "Layer=DataAccess"

# Exclude tests requiring external dependencies
dotnet test --filter "Category=Integration&ExternalDependency!~WinGet"
```

## Performance Testing Guidelines

### Measuring Performance

```csharp
public class PerformanceIntegrationTests
{
    [Fact]
    [Trait("Category", "Performance")]
    public async Task PackageSearch_CompletesWithinTimeLimit()
    {
        // Arrange
        var manager = new PackageManager();
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var results = await manager.SearchPackagesAsync("test");
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Search took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [Trait("Category", "Performance")]
    public async Task BulkPackageLoad_ScalesLinearly(int packageCount)
    {
        // Arrange
        var loader = new PackageLoader();
        var packages = CreateTestPackages(packageCount);
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        await loader.LoadPackagesAsync(packages);
        stopwatch.Stop();
        
        // Assert - roughly linear scaling
        var timePerPackage = stopwatch.ElapsedMilliseconds / (double)packageCount;
        Assert.True(timePerPackage < 10, 
            $"Time per package: {timePerPackage}ms (expected < 10ms)");
    }
}
```

### Memory Testing

```csharp
[Fact]
[Trait("Category", "Performance")]
public void LoadLargeDataset_DoesNotExceedMemoryLimit()
{
    // Arrange
    var initialMemory = GC.GetTotalMemory(true);
    var maxMemoryIncrease = 100 * 1024 * 1024; // 100 MB
    
    // Act
    var largeDataset = LoadLargeDataset();
    var finalMemory = GC.GetTotalMemory(false);
    
    // Assert
    var memoryIncrease = finalMemory - initialMemory;
    Assert.True(memoryIncrease < maxMemoryIncrease,
        $"Memory increased by {memoryIncrease / 1024 / 1024}MB, expected < 100MB");
}
```

## Load Testing Approaches

### Concurrent Request Testing

```csharp
public class LoadTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [Trait("Category", "Load")]
    public async Task ConcurrentPackageInstalls_HandlesLoad(int concurrentRequests)
    {
        // Arrange
        var manager = new PackageManager();
        var packages = CreateTestPackages(concurrentRequests);
        
        // Act
        var tasks = packages.Select(pkg => manager.InstallAsync(pkg));
        var sw = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        sw.Stop();
        
        // Assert
        Assert.All(results, result => Assert.True(result.Success));
        var avgTimePerRequest = sw.ElapsedMilliseconds / (double)concurrentRequests;
        
        // Log performance metrics
        Console.WriteLine($"Completed {concurrentRequests} concurrent requests in {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average time per request: {avgTimePerRequest}ms");
    }
}
```

### Stress Testing

```csharp
[Fact]
[Trait("Category", "Stress")]
public async Task ContinuousLoad_MaintainsStability()
{
    // Arrange
    var manager = new PackageManager();
    var duration = TimeSpan.FromMinutes(5);
    var requestsPerSecond = 10;
    var startTime = DateTime.UtcNow;
    var successCount = 0;
    var failureCount = 0;
    
    // Act
    while (DateTime.UtcNow - startTime < duration)
    {
        var tasks = Enumerable.Range(0, requestsPerSecond)
            .Select(_ => manager.GetPackagesAsync());
        
        var results = await Task.WhenAll(tasks);
        successCount += results.Count(r => r != null);
        failureCount += results.Count(r => r == null);
        
        await Task.Delay(1000); // Wait 1 second
    }
    
    // Assert
    var successRate = successCount / (double)(successCount + failureCount);
    Assert.True(successRate > 0.99, 
        $"Success rate: {successRate:P2}, expected > 99%");
}
```

## CI/CD Integration

### Running Integration Tests in CI

```yaml
# .github/workflows/integration-tests.yml
name: Integration Tests

on:
  pull_request:
    branches: [ main ]
  push:
    branches: [ main ]

jobs:
  integration-tests:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v5
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      working-directory: src
      run: dotnet restore
    
    - name: Run Integration Tests
      working-directory: src
      run: dotnet test --filter "Category=Integration" --verbosity normal
    
    - name: Run Performance Tests
      working-directory: src
      run: dotnet test --filter "Category=Performance" --verbosity normal
```

### Conditional Test Execution

```csharp
public class ConditionalIntegrationTests
{
    [Fact]
    public async Task TestRequiringExternalService()
    {
        // Skip in CI environments where service isn't available
        if (IsRunningInCI() && !IsServiceAvailable())
        {
            throw new SkipException("External service not available in CI");
        }
        
        // Run test
    }

    private bool IsRunningInCI()
    {
        return Environment.GetEnvironmentVariable("CI") == "true";
    }

    private bool IsServiceAvailable()
    {
        // Check if external service is accessible
        return CheckServiceHealth();
    }
}
```

## Best Practices

### 1. Keep Tests Independent

```csharp
// ✅ Good - Each test has its own data
[Fact]
public void Test1()
{
    var context = CreateTestContext();
    // Use context
    context.Dispose();
}

[Fact]
public void Test2()
{
    var context = CreateTestContext();
    // Use context
    context.Dispose();
}

// ❌ Bad - Shared state between tests
private static DatabaseContext _sharedContext;

[Fact]
public void Test1()
{
    _sharedContext.Add(data); // Affects Test2
}

[Fact]
public void Test2()
{
    var data = _sharedContext.GetData(); // Depends on Test1
}
```

### 2. Clean Up Resources

```csharp
public class IntegrationTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly string _tempDirectory;

    public IntegrationTests()
    {
        _context = CreateContext();
        _tempDirectory = CreateTempDirectory();
    }

    [Fact]
    public void Test()
    {
        // Use resources
    }

    public void Dispose()
    {
        _context.Dispose();
        Directory.Delete(_tempDirectory, true);
    }
}
```

### 3. Use Realistic Test Data

```csharp
// ✅ Good - Realistic data
[Fact]
public void ProcessPackage_WithRealWorldData_Works()
{
    var package = new Package
    {
        Id = "Microsoft.VisualStudioCode",
        Name = "Visual Studio Code",
        Version = "1.85.0",
        Source = "WinGet",
        Publisher = "Microsoft Corporation"
    };
    
    var result = processor.Process(package);
    Assert.True(result.Success);
}

// ❌ Bad - Unrealistic data
[Fact]
public void ProcessPackage_WithMinimalData_Works()
{
    var package = new Package { Id = "a", Name = "b" };
    var result = processor.Process(package);
    // May pass but doesn't represent real usage
}
```

### 4. Test Error Scenarios

```csharp
[Fact]
public async Task PackageInstall_WhenDiskFull_ReturnsError()
{
    // Simulate low disk space scenario
    var result = await manager.InstallPackageAsync(package);
    
    Assert.False(result.Success);
    Assert.Contains("insufficient disk space", result.Error.ToLower());
}

[Fact]
public async Task PackageSearch_WhenNetworkDown_HandlesGracefully()
{
    // Simulate network unavailability
    var mockHttp = CreateOfflineHttpClient();
    var manager = new PackageManager(mockHttp);
    
    var result = await manager.SearchAsync("test");
    
    Assert.NotNull(result);
    Assert.Empty(result.Packages);
    Assert.True(result.IsOfflineMode);
}
```

### 5. Document External Dependencies

```csharp
/// <summary>
/// Integration tests for WinGet package manager.
/// REQUIRES: WinGet to be installed on the test machine.
/// REQUIRES: Network access to WinGet repositories.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Requires", "WinGet")]
[Trait("Requires", "Network")]
public class WinGetIntegrationTests
{
    // Tests...
}
```

## Common Pitfalls

### 1. Over-Mocking

```csharp
// ❌ Bad - Everything is mocked, nothing is integrated
[Fact]
public void OverMockedTest()
{
    var mockA = new Mock<IServiceA>();
    var mockB = new Mock<IServiceB>();
    var mockC = new Mock<IServiceC>();
    var mockD = new Mock<IServiceD>();
    
    // This is really a unit test, not an integration test
}

// ✅ Good - Real integration, strategic mocking
[Fact]
public void RealIntegration()
{
    var realServiceA = new ServiceA();
    var realServiceB = new ServiceB();
    var mockExternalApi = new Mock<IExternalApi>(); // Only mock external dependency
    
    var system = new SystemUnderTest(realServiceA, realServiceB, mockExternalApi.Object);
    // Test real integration
}
```

### 2. Slow Test Suites

```csharp
// ❌ Bad - Every test hits real database
[Fact]
public void SlowTest1()
{
    var db = new SqlDatabase("production-connection-string");
    // Slow test
}

// ✅ Good - Use in-memory database or optimize
[Fact]
public void FastTest1()
{
    var db = new InMemoryDatabase();
    // Fast test
}
```

### 3. Flaky Tests

```csharp
// ❌ Bad - Timing-dependent test
[Fact]
public async Task FlakyTest()
{
    StartBackgroundOperation();
    await Task.Delay(100); // Might not be enough!
    Assert.True(operationCompleted);
}

// ✅ Good - Wait for specific condition
[Fact]
public async Task ReliableTest()
{
    StartBackgroundOperation();
    await WaitUntilAsync(() => operationCompleted, timeout: TimeSpan.FromSeconds(5));
    Assert.True(operationCompleted);
}
```

## Resources

### Recommended Libraries

```xml
<ItemGroup>
  <!-- Mocking -->
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="NSubstitute" Version="5.1.0" />
  
  <!-- In-Memory Database -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  
  <!-- HTTP Mocking -->
  <PackageReference Include="RichardSzalay.MockHttp" Version="7.0.0" />
  
  <!-- Assertions -->
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  
  <!-- Test Data Generation -->
  <PackageReference Include="Bogus" Version="35.4.0" />
</ItemGroup>
```

### Documentation

- [Integration Testing in .NET](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Moq Documentation](https://github.com/moq/moq4)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [xUnit Documentation](https://xunit.net/)

### Books

- "Growing Object-Oriented Software, Guided by Tests" by Steve Freeman & Nat Pryce
- "Working Effectively with Legacy Code" by Michael Feathers
- "Test Driven Development: By Example" by Kent Beck

## Examples

See `/examples/testing/integration-test-examples/` for comprehensive examples:
- Database integration tests
- File system integration tests
- External service integration tests
- Package manager integration tests
- Process execution tests

## Next Steps

- Review [Unit Testing Guide](./unit-testing-guide.md)
- Review [UI Automation Guide](./ui-automation-guide.md)
- Explore integration test examples
- Set up integration test environment
- Configure CI/CD for integration tests
