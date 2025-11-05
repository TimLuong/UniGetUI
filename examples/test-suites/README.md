# Test Suites Example

Comprehensive testing examples for .NET applications using xUnit.

## Test Types Covered

- **Unit Tests** - Individual component testing
- **Integration Tests** - Component interaction testing
- **Mock Implementations** - Testing with mocks
- **Test Fixtures** - Shared test context
- **Code Coverage** - Coverage analysis
- **Performance Tests** - Benchmarking

## Project Structure

```
tests/
├── Unit/
│   ├── Services/
│   │   ├── PackageServiceTests.cs
│   │   └── TaskRecyclerTests.cs
│   └── ViewModels/
│       └── MainViewModelTests.cs
├── Integration/
│   ├── ApiIntegrationTests.cs
│   └── DatabaseIntegrationTests.cs
├── Mocks/
│   ├── MockPackageService.cs
│   └── MockHttpMessageHandler.cs
└── Fixtures/
    └── DatabaseFixture.cs
```

## Unit Tests

### Service Testing

```csharp
public class PackageServiceFactoryTests
{
    [Fact]
    public void GetService_SameType_ReturnsSameInstance()
    {
        // Arrange
        var factory = new PackageServiceFactory();
        
        // Act
        var service1 = factory.GetService("winget");
        var service2 = factory.GetService("winget");
        
        // Assert
        Assert.Same(service1, service2);
    }
    
    [Fact]
    public void GetService_DifferentTypes_ReturnsDifferentInstances()
    {
        // Arrange
        var factory = new PackageServiceFactory();
        
        // Act
        var wingetService = factory.GetService("winget");
        var scoopService = factory.GetService("scoop");
        
        // Assert
        Assert.NotSame(wingetService, scoopService);
    }
    
    [Theory]
    [InlineData("winget", "WinGet")]
    [InlineData("scoop", "Scoop")]
    [InlineData("chocolatey", "Chocolatey")]
    public void GetService_ValidTypes_ReturnsCorrectServiceName(
        string serviceType, string expectedName)
    {
        // Arrange
        var factory = new PackageServiceFactory();
        
        // Act
        var service = factory.GetService(serviceType);
        
        // Assert
        Assert.Equal(expectedName, service.ServiceName);
    }
    
    [Fact]
    public void GetService_InvalidType_ThrowsArgumentException()
    {
        // Arrange
        var factory = new PackageServiceFactory();
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.GetService("invalid"));
    }
}
```

### Task Recycler Tests

```csharp
public class TaskRecyclerTests
{
    [Fact]
    public async Task RunOrAttachAsync_SameMethod_ReturnsSameResult()
    {
        // Arrange
        int callCount = 0;
        Func<int> method = () =>
        {
            Interlocked.Increment(ref callCount);
            Thread.Sleep(100);
            return 42;
        };
        
        // Act
        var task1 = TaskRecycler<int>.RunOrAttachAsync(method);
        var task2 = TaskRecycler<int>.RunOrAttachAsync(method);
        
        var result1 = await task1;
        var result2 = await task2;
        
        // Assert
        Assert.Equal(42, result1);
        Assert.Equal(42, result2);
        Assert.Equal(1, callCount); // Method called only once
    }
    
    [Fact]
    public async Task RunOrAttachAsync_ConcurrentCalls_ExecutesOnce()
    {
        // Arrange
        int executionCount = 0;
        Func<int> slowMethod = () =>
        {
            Interlocked.Increment(ref executionCount);
            Thread.Sleep(1000);
            return 123;
        };
        
        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => TaskRecycler<int>.RunOrAttachAsync(slowMethod))
            .ToList();
        
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.All(results, r => Assert.Equal(123, r));
        Assert.Equal(1, executionCount); // Executed only once despite 10 calls
    }
    
    [Fact]
    public async Task RunOrAttachAsync_WithCaching_CachesResult()
    {
        // Arrange
        int callCount = 0;
        Func<string> method = () =>
        {
            Interlocked.Increment(ref callCount);
            return "cached";
        };
        
        // Act
        var result1 = await TaskRecycler<string>.RunOrAttachAsync(method, cacheTimeSecs: 2);
        await Task.Delay(500); // Wait but cache still valid
        var result2 = await TaskRecycler<string>.RunOrAttachAsync(method, cacheTimeSecs: 2);
        
        // Assert
        Assert.Equal("cached", result1);
        Assert.Equal("cached", result2);
        Assert.Equal(1, callCount); // Used cached result
    }
}
```

### ViewModel Tests

```csharp
public class MainViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Act
        var viewModel = new MainViewModel();
        
        // Assert
        Assert.NotNull(viewModel.Packages);
        Assert.Equal("Ready", viewModel.StatusMessage);
        Assert.NotNull(viewModel.LoadPackagesCommand);
    }
    
    [Fact]
    public async Task LoadPackagesAsync_UpdatesPackagesCollection()
    {
        // Arrange
        var viewModel = new MainViewModel();
        
        // Act
        await viewModel.LoadPackagesCommand.ExecuteAsync(null);
        
        // Assert
        Assert.True(viewModel.Packages.Count > 0);
        Assert.Contains("package", viewModel.StatusMessage.ToLower());
    }
    
    [Fact]
    public void SelectedPackage_PropertyChanged_RaisesEvent()
    {
        // Arrange
        var viewModel = new MainViewModel();
        var package = new Package { Name = "Test" };
        bool eventRaised = false;
        
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedPackage))
                eventRaised = true;
        };
        
        // Act
        viewModel.SelectedPackage = package;
        
        // Assert
        Assert.True(eventRaised);
        Assert.Equal(package, viewModel.SelectedPackage);
    }
}
```

## Integration Tests

### API Integration Tests

```csharp
public class GitHubApiIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly HttpClient _client;
    
    public GitHubApiIntegrationTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetUser_ValidUsername_ReturnsUser()
    {
        // Act
        var response = await _client.GetAsync("/api/github/users/octocat");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(user);
        Assert.Equal("octocat", user.Login);
    }
    
    [Fact]
    public async Task GetUser_InvalidUsername_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/github/users/this-user-does-not-exist-12345");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

### Database Integration Tests

```csharp
public class PackageRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public PackageRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task AddPackage_ValidPackage_AddsToDatabase()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new PackageRepository(context);
        var package = new Package
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Package",
            Version = "1.0.0"
        };
        
        // Act
        await repository.AddAsync(package);
        await context.SaveChangesAsync();
        
        // Assert
        var saved = await repository.GetByIdAsync(package.Id);
        Assert.NotNull(saved);
        Assert.Equal(package.Name, saved.Name);
    }
    
    [Fact]
    public async Task GetInstalledPackages_ReturnsOnlyInstalled()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new PackageRepository(context);
        
        await repository.AddRangeAsync(new[]
        {
            new Package { Id = "1", Name = "Installed", IsInstalled = true },
            new Package { Id = "2", Name = "Not Installed", IsInstalled = false }
        });
        await context.SaveChangesAsync();
        
        // Act
        var installed = await repository.GetInstalledPackagesAsync();
        
        // Assert
        Assert.Single(installed);
        Assert.Equal("Installed", installed.First().Name);
    }
}
```

## Mock Implementations

### Mock Package Service

```csharp
public class MockPackageService : IPackageService
{
    private readonly List<Package> _packages = new();
    
    public string ServiceName => "Mock";
    
    public void AddPackage(Package package)
    {
        _packages.Add(package);
    }
    
    public Task<List<Package>> GetAvailablePackagesAsync()
    {
        return Task.FromResult(_packages);
    }
    
    public Task<bool> InstallPackageAsync(Package package)
    {
        package.IsInstalled = true;
        return Task.FromResult(true);
    }
    
    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(true);
    }
}
```

### Mock HTTP Message Handler

```csharp
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _responses = new();
    
    public void AddResponse(string url, HttpResponseMessage response)
    {
        _responses[url] = response;
    }
    
    public void AddJsonResponse<T>(string url, T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(data);
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        _responses[url] = response;
    }
    
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? "";
        
        if (_responses.TryGetValue(url, out var response))
        {
            return Task.FromResult(response);
        }
        
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
```

## Test Fixtures

### Database Fixture

```csharp
public class DatabaseFixture : IDisposable
{
    private readonly string _connectionString;
    
    public DatabaseFixture()
    {
        _connectionString = $"DataSource=:memory:";
        
        // Create database schema
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }
    
    public PackageDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PackageDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        
        return new PackageDbContext(options);
    }
    
    public void Dispose()
    {
        // Cleanup
    }
}
```

## Code Coverage

### Running with Coverage

```bash
# Install coverlet
dotnet add package coverlet.msbuild

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report (requires ReportGenerator)
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

### Coverage Configuration

```xml
<PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>opencover</CoverletOutputFormat>
    <Exclude>[*]*.Migrations.*</Exclude>
    <ExcludeByAttribute>GeneratedCode,ExcludeFromCodeCoverage</ExcludeByAttribute>
</PropertyGroup>
```

## Performance Tests

### Benchmark Tests

```csharp
[MemoryDiagnoser]
public class TaskRecyclerBenchmarks
{
    [Benchmark]
    public async Task WithoutTaskRecycler()
    {
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => ExpensiveOperation()))
            .ToList();
        
        await Task.WhenAll(tasks);
    }
    
    [Benchmark]
    public async Task WithTaskRecycler()
    {
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => TaskRecycler<int>.RunOrAttachAsync(ExpensiveOperation))
            .ToList();
        
        await Task.WhenAll(tasks);
    }
    
    private int ExpensiveOperation()
    {
        Thread.Sleep(10);
        return 42;
    }
}
```

## Best Practices

### 1. AAA Pattern (Arrange-Act-Assert)
```csharp
[Fact]
public void Test_FollowsAAAPattern()
{
    // Arrange - Set up test data and dependencies
    var service = new PackageService();
    var package = new Package { Name = "Test" };
    
    // Act - Execute the code under test
    var result = service.ProcessPackage(package);
    
    // Assert - Verify the outcome
    Assert.NotNull(result);
}
```

### 2. One Assert Per Test (Guideline)
```csharp
[Fact]
public void Package_HasValidName()
{
    var package = new Package { Name = "TestPackage" };
    Assert.Equal("TestPackage", package.Name);
}

[Fact]
public void Package_HasValidVersion()
{
    var package = new Package { Version = "1.0.0" };
    Assert.Equal("1.0.0", package.Version);
}
```

### 3. Test Naming Convention
```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public void GetPackage_ValidId_ReturnsPackage() { }

[Fact]
public void GetPackage_InvalidId_ReturnsNull() { }

[Fact]
public void InstallPackage_AlreadyInstalled_ThrowsException() { }
```

### 4. Use Theory for Multiple Test Cases
```csharp
[Theory]
[InlineData(null, false)]
[InlineData("", false)]
[InlineData("validId", true)]
public void IsValidPackageId_VariousInputs_ReturnsExpected(
    string packageId, bool expected)
{
    var result = InputValidator.IsValidPackageId(packageId);
    Assert.Equal(expected, result);
}
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "FullyQualifiedName~PackageServiceFactoryTests"

# Run tests in parallel
dotnet test --parallel

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## Test Organization

```
tests/
├── UniGetUI.Tests.csproj
├── Unit/
│   ├── Core/
│   │   ├── TaskRecyclerTests.cs
│   │   └── ObservableQueueTests.cs
│   ├── Services/
│   │   ├── PackageServiceTests.cs
│   │   └── InstallationServiceTests.cs
│   └── ViewModels/
│       └── MainViewModelTests.cs
├── Integration/
│   ├── ApiTests.cs
│   └── DatabaseTests.cs
├── Mocks/
│   └── MockServices.cs
└── Fixtures/
    └── TestFixtures.cs
```

## Summary

This test suite demonstrates:
- ✅ **Unit Testing** - Testing individual components
- ✅ **Integration Testing** - Testing component interactions
- ✅ **Mocking** - Isolating dependencies
- ✅ **Fixtures** - Sharing test context
- ✅ **Code Coverage** - Measuring test coverage
- ✅ **Performance Testing** - Benchmarking code
- ✅ **Best Practices** - Following testing conventions

## License

Part of the UniGetUI project examples.
