# Unit Testing Guide

## Overview

This guide provides comprehensive standards, patterns, and best practices for writing unit tests in Windows applications using .NET and C#. UniGetUI follows a test-driven development approach with a minimum code coverage target of 80%.

## Testing Framework

### xUnit

UniGetUI uses **xUnit** as the primary testing framework. xUnit is the recommended framework for .NET applications due to its modern design, extensibility, and excellent integration with .NET tooling.

#### Why xUnit?

- Modern, extensible architecture
- Excellent .NET Core/.NET 8 support
- Parallel test execution by default
- Clean, attribute-based test declaration
- Strong community support and active development
- Built-in support for data-driven tests via `[Theory]` and `[InlineData]`

### Alternative Frameworks

While xUnit is preferred, the following frameworks are also acceptable for specific scenarios:

#### NUnit
- Legacy codebase migration
- Teams already familiar with NUnit
- Scenarios requiring advanced setup/teardown patterns

#### MSTest
- Enterprise environments with existing MSTest infrastructure
- Visual Studio integration requirements
- Microsoft-specific tooling needs

## Project Structure

### Test Project Organization

Test projects should mirror the structure of the code they test:

```
src/
├── UniGetUI.Core.Classes/
│   └── TaskRecycler.cs
└── UniGetUI.Core.Classes.Tests/
    ├── TaskRecyclerTests.cs
    └── UniGetUI.Core.Classes.Tests.csproj
```

### Test Project Naming Convention

- **Pattern**: `{ProjectName}.Tests`
- **Examples**:
  - `UniGetUI.Core.Classes.Tests`
  - `UniGetUI.PackageEngine.Serializable.Tests`
  - `UniGetUI.Core.Tools.Tests`

### Test File Naming Convention

- **Pattern**: `{ClassUnderTest}Tests.cs`
- **Examples**:
  - `TaskRecyclerTests.cs` (tests `TaskRecycler.cs`)
  - `ToolsTests.cs` (tests `CoreTools.cs`)
  - `PersonTests.cs` (tests `Person.cs`)

## Test Project Setup

### Standard .csproj Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.abstractions" Version="2.0.3" />
    <PackageReference Include="xunit.analyzers" Version="1.24.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\{ProjectUnderTest}\{ProjectUnderTest}.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="..\SharedAssemblyInfo.cs" Link="SharedAssemblyInfo.cs" />
  </ItemGroup>
</Project>
```

### Key Dependencies

- **Microsoft.NET.Test.Sdk**: .NET test platform
- **xunit**: Core xUnit framework
- **xunit.runner.visualstudio**: Visual Studio integration
- **coverlet.collector**: Code coverage collection

## Writing Unit Tests

### Basic Test Structure

```csharp
namespace UniGetUI.Core.Classes.Tests;

public class TaskRecyclerTests
{
    [Fact]
    public void TestMethodName_Scenario_ExpectedBehavior()
    {
        // Arrange - Set up test data and preconditions
        var inputValue = 42;
        
        // Act - Execute the code under test
        var result = MethodUnderTest(inputValue);
        
        // Assert - Verify the expected outcome
        Assert.Equal(expectedValue, result);
    }
}
```

### Test Naming Conventions

Use descriptive names that clearly indicate:
1. What is being tested
2. Under what conditions
3. What the expected result is

**Pattern**: `{MethodName}_{Scenario}_{ExpectedBehavior}`

**Examples**:
```csharp
[Fact]
public void FormatAsName_WithLowercaseInput_ReturnsCapitalizedString()

[Fact]
public void WhichAsync_WithExistingFile_ReturnsTrue()

[Fact]
public void WhichAsync_WithNonExistingFile_ReturnsFalse()

[Fact]
public void TaskRecycler_StaticMethod_CachesResults()
```

### The AAA Pattern

**Arrange-Act-Assert** is the standard pattern for structuring unit tests:

```csharp
[Fact]
public void RandomString_GeneratesUniqueStrings()
{
    // Arrange
    int length = 10;
    
    // Act
    string string1 = CoreTools.RandomString(length);
    string string2 = CoreTools.RandomString(length);
    
    // Assert
    Assert.Equal(length, string1.Length);
    Assert.Equal(length, string2.Length);
    Assert.NotEqual(string1, string2);
}
```

### Facts vs Theories

#### [Fact] - Single Test Case

Use `[Fact]` for tests with a single set of inputs:

```csharp
[Fact]
public async Task TestWhichFunctionForExistingFile()
{
    Tuple<bool, string> result = await CoreTools.WhichAsync("cmd.exe");
    Assert.True(result.Item1);
    Assert.True(File.Exists(result.Item2));
}
```

#### [Theory] - Data-Driven Tests

Use `[Theory]` with `[InlineData]` for parameterized tests:

```csharp
[Theory]
[InlineData("packagename", "Packagename")]
[InlineData("PackageName", "PackageName")]
[InlineData("package-Name", "Package Name")]
[InlineData("PACKAGENAME", "PACKAGENAME")]
public void TestFormatAsName(string id, string name)
{
    Assert.Equal(name, CoreTools.FormatAsName(id));
}
```

**Benefits**:
- Reduces code duplication
- Easy to add new test cases
- Clear data-driven intent
- Maintains single assertion per theory row

### Common xUnit Assertions

```csharp
// Equality
Assert.Equal(expected, actual);
Assert.NotEqual(expected, actual);

// Boolean
Assert.True(condition);
Assert.False(condition);

// Null checks
Assert.Null(value);
Assert.NotNull(value);

// Strings
Assert.StartsWith("prefix", actual);
Assert.EndsWith("suffix", actual);
Assert.Contains("substring", actual);
Assert.Empty(collection);
Assert.NotEmpty(collection);

// Collections
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.All(collection, item => Assert.NotNull(item));

// Exceptions
Assert.Throws<InvalidOperationException>(() => MethodThatThrows());
var ex = Assert.Throws<ArgumentException>(() => MethodThatThrows());
Assert.Equal("expectedMessage", ex.Message);

// Ranges
Assert.InRange(actual, low, high);
Assert.NotInRange(actual, low, high);
```

## Async Testing

### Testing Async Methods

```csharp
[Fact]
public async Task TestAsyncMethod_ReturnsExpectedResult()
{
    // Arrange
    var service = new MyService();
    
    // Act
    var result = await service.GetDataAsync();
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expectedValue, result.Value);
}
```

### Testing Task-Based Code

```csharp
[Fact]
public async Task TestTaskRecycler_CachesTaskResults()
{
    // Arrange & Act
    var task1 = TaskRecycler<int>.RunOrAttachAsync(MySlowMethod);
    var task2 = TaskRecycler<int>.RunOrAttachAsync(MySlowMethod);
    
    int result1 = await task1;
    int result2 = await task2;
    
    // Assert - Same task should return same result
    Assert.Equal(result1, result2);
}
```

### Avoiding Thread.Sleep in Tests

Instead of `Thread.Sleep`, use proper async patterns:

```csharp
// ❌ Bad - Blocks thread
[Fact]
public void BadTest()
{
    Thread.Sleep(1000);
    Assert.True(condition);
}

// ✅ Good - Uses async properly
[Fact]
public async Task GoodTest()
{
    await Task.Delay(1000);
    Assert.True(condition);
}
```

**Exception**: `Thread.Sleep` is acceptable when testing caching/timeout behavior explicitly:

```csharp
[Fact]
public async Task TestCache_ExpiresAfterTimeout()
{
    var result1 = await GetCachedValue();
    
    // Wait for cache to expire
    Thread.Sleep(3000);
    
    var result2 = await GetCachedValue();
    Assert.NotEqual(result1, result2);
}
```

## Test Coverage Requirements

### Minimum Coverage Standards

- **Overall Project**: 80% minimum code coverage
- **Core Libraries**: 85% minimum
- **Critical Path Code**: 90% minimum
- **New Code**: 80% minimum (enforced in PR reviews)

### Coverage Tools

#### Coverlet

Coverlet is integrated via `coverlet.collector` package and works seamlessly with `dotnet test`:

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage with detailed report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

#### Coverage Report Formats

```bash
# Generate HTML report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html
```

### Coverage Exclusions

Exclude from coverage:
- Auto-generated code
- Third-party libraries
- UI event handlers (covered by UI tests)
- Platform-specific interop code

```csharp
[ExcludeFromCodeCoverage]
public class AutoGeneratedClass
{
    // ...
}
```

### Measuring Coverage

```bash
# Run tests with coverage
cd src
dotnet test --collect:"XPlat Code Coverage" --verbosity normal

# View coverage summary in CI/CD
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Best Practices

### 1. Test Independence

Each test should be completely independent:

```csharp
// ✅ Good - Independent test
[Fact]
public void Test_IndependentScenario()
{
    var instance = new MyClass(); // Fresh instance
    var result = instance.DoSomething();
    Assert.Equal(expected, result);
}

// ❌ Bad - Shared state
private static MyClass _sharedInstance = new();

[Fact]
public void Test_DependentScenario()
{
    _sharedInstance.DoSomething(); // Affects other tests
}
```

### 2. Single Responsibility

Each test should verify one behavior:

```csharp
// ✅ Good - Tests one thing
[Fact]
public void FormatAsName_RemovesHyphens()
{
    Assert.Equal("Package Name", CoreTools.FormatAsName("package-name"));
}

[Fact]
public void FormatAsName_CapitalizesFirstLetter()
{
    Assert.Equal("Packagename", CoreTools.FormatAsName("packagename"));
}

// ❌ Bad - Tests multiple things
[Fact]
public void FormatAsName_ProcessesInput()
{
    Assert.Equal("Package Name", CoreTools.FormatAsName("package-name"));
    Assert.Equal("Packagename", CoreTools.FormatAsName("packagename"));
    Assert.Equal("PackageName", CoreTools.FormatAsName("PackageName"));
}
```

### 3. Avoid Logic in Tests

Tests should be simple and straightforward:

```csharp
// ✅ Good - No logic
[Theory]
[InlineData(5, 10)]
[InlineData(10, 20)]
public void Multiply_ReturnsCorrectValue(int input, int expected)
{
    Assert.Equal(expected, Calculate.Multiply(input, 2));
}

// ❌ Bad - Logic in test
[Theory]
[InlineData(5)]
[InlineData(10)]
public void Multiply_ReturnsCorrectValue(int input)
{
    int expected = input * 2; // Duplicates production logic
    Assert.Equal(expected, Calculate.Multiply(input, 2));
}
```

### 4. Use Descriptive Variable Names

```csharp
// ✅ Good
[Fact]
public void GetUser_WithValidId_ReturnsUser()
{
    var validUserId = 123;
    var expectedUserName = "John Doe";
    
    var actualUser = userService.GetUser(validUserId);
    
    Assert.Equal(expectedUserName, actualUser.Name);
}

// ❌ Bad
[Fact]
public void Test1()
{
    var x = 123;
    var y = "John Doe";
    var z = userService.GetUser(x);
    Assert.Equal(y, z.Name);
}
```

### 5. Test Edge Cases

Always test boundary conditions:

```csharp
[Theory]
[InlineData(0)]        // Minimum
[InlineData(1)]        // Minimum + 1
[InlineData(100)]      // Normal case
[InlineData(int.MaxValue)] // Maximum
public void ProcessValue_HandlesRange(int value)
{
    var result = processor.ProcessValue(value);
    Assert.True(result.IsValid);
}
```

### 6. Test Exception Handling

```csharp
[Fact]
public void ProcessInput_WithNullInput_ThrowsArgumentNullException()
{
    var processor = new InputProcessor();
    
    var exception = Assert.Throws<ArgumentNullException>(
        () => processor.ProcessInput(null)
    );
    
    Assert.Equal("input", exception.ParamName);
}
```

### 7. Use Test Fixtures for Shared Setup

When multiple tests need the same setup:

```csharp
public class DatabaseTests : IDisposable
{
    private readonly DatabaseContext _context;

    public DatabaseTests()
    {
        // Constructor runs before each test
        _context = new DatabaseContext();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void AddUser_SavesToDatabase()
    {
        // Use _context
    }

    public void Dispose()
    {
        // Cleanup runs after each test
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### 8. Collection Fixtures for Expensive Setup

For expensive setup shared across multiple test classes:

```csharp
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

public class DatabaseFixture : IDisposable
{
    public DatabaseContext Context { get; }

    public DatabaseFixture()
    {
        Context = new DatabaseContext();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}

[Collection("Database collection")]
public class UserTests
{
    private readonly DatabaseFixture _fixture;

    public UserTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

## Running Tests

### Command Line

```bash
# Run all tests
cd src
dotnet test

# Run tests for specific project
dotnet test UniGetUI.Core.Classes.Tests/UniGetUI.Core.Classes.Tests.csproj

# Run tests with verbosity
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "FullyQualifiedName~TaskRecyclerTests.TestTaskRecycler_Static_Int"

# Run tests by category
dotnet test --filter "Category=Integration"

# Run tests in parallel (default)
dotnet test --parallel

# Run tests sequentially
dotnet test -- xUnit.ParallelizeTestCollections=false
```

### Visual Studio

1. **Test Explorer**: `Test` → `Test Explorer`
2. **Run All**: Click "Run All" button
3. **Run Specific**: Right-click test → "Run"
4. **Debug Test**: Right-click test → "Debug"

### Continuous Integration

Tests run automatically on:
- Every push to `main` branch
- Every pull request
- Manual workflow dispatch

See `.github/workflows/dotnet-test.yml` for CI configuration.

## Test Categories and Traits

Use traits to categorize tests:

```csharp
[Fact]
[Trait("Category", "Unit")]
public void UnitTest() { }

[Fact]
[Trait("Category", "Integration")]
public void IntegrationTest() { }

[Fact]
[Trait("Speed", "Slow")]
public void SlowTest() { }
```

Run specific categories:

```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Speed!=Slow"
```

## Common Pitfalls

### 1. Testing Implementation Instead of Behavior

```csharp
// ❌ Bad - Tests implementation
[Fact]
public void GetUser_CallsDatabaseExactlyOnce()
{
    var mockDb = new Mock<IDatabase>();
    var service = new UserService(mockDb.Object);
    
    service.GetUser(1);
    
    mockDb.Verify(db => db.Query(It.IsAny<string>()), Times.Once);
}

// ✅ Good - Tests behavior
[Fact]
public void GetUser_WithValidId_ReturnsCorrectUser()
{
    var service = new UserService(new InMemoryDatabase());
    
    var user = service.GetUser(1);
    
    Assert.Equal("John Doe", user.Name);
}
```

### 2. Overly Complex Setup

```csharp
// ❌ Bad - Too complex
[Fact]
public void ComplexTest()
{
    var config = CreateComplexConfig();
    var dependencies = SetupMultipleDependencies();
    var state = InitializeComplexState();
    // ... 50 more lines of setup
}

// ✅ Good - Simple and focused
[Fact]
public void SimpleTest()
{
    var service = new MyService();
    var result = service.DoSomething();
    Assert.NotNull(result);
}
```

### 3. Testing Multiple Concerns

Split tests that verify multiple behaviors:

```csharp
// ❌ Bad
[Fact]
public void ProcessUser_DoesEverything()
{
    var user = processor.ProcessUser(input);
    Assert.NotNull(user);
    Assert.Equal("John", user.FirstName);
    Assert.True(user.IsActive);
    Assert.Contains(user, database.Users);
}

// ✅ Good - Split into focused tests
[Fact]
public void ProcessUser_ReturnsNonNullUser() { }

[Fact]
public void ProcessUser_SetsFirstNameCorrectly() { }

[Fact]
public void ProcessUser_SetsActiveStatus() { }

[Fact]
public void ProcessUser_SavesToDatabase() { }
```

## Resources

### Official Documentation
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [.NET Testing](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)

### Books
- "xUnit Test Patterns" by Gerard Meszaros
- "The Art of Unit Testing" by Roy Osherove
- "Unit Testing Principles, Practices, and Patterns" by Vladimir Khorikov

### Tools
- [xUnit](https://xunit.net/) - Testing framework
- [FluentAssertions](https://fluentassertions.com/) - Fluent assertion library
- [Coverlet](https://github.com/coverlet-coverage/coverlet) - Code coverage
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator) - Coverage reports

## Examples

See `/examples/testing/unit-test-examples/` for comprehensive examples demonstrating:
- Basic unit tests
- Data-driven tests with theories
- Async testing patterns
- Mock usage
- Complex object testing
- Collection testing

## Next Steps

- Review [Integration Testing Guide](./integration-testing-guide.md)
- Review [UI Automation Guide](./ui-automation-guide.md)
- Explore example test projects in `/examples/testing/`
- Set up code coverage in your IDE
