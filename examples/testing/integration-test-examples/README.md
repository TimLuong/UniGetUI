# Integration Test Examples

This directory contains comprehensive examples of integration tests demonstrating how to test interactions between multiple components, external dependencies, and infrastructure.

## Examples Included

1. **DatabaseIntegrationTests.cs** - Testing with in-memory databases
2. **FileSystemIntegrationTests.cs** - Testing file operations
3. **ProcessIntegrationTests.cs** - Testing external process execution
4. **ServiceIntegrationTests.cs** - Testing service layer integration

## Running the Examples

```bash
cd examples/testing/integration-test-examples
dotnet test
```

## Key Concepts Demonstrated

### 1. Database Integration

- Using in-memory databases for testing
- Proper setup and teardown
- Testing CRUD operations
- Transaction handling

### 2. File System Testing

- Creating temporary test directories
- Testing file read/write operations
- Cleanup after tests
- Testing file system edge cases

### 3. External Process Testing

- Executing command-line tools
- Capturing output and errors
- Testing exit codes
- Handling timeouts

### 4. Service Integration

- Testing multiple components together
- Mocking only external dependencies
- Testing real workflows
- Verifying component interactions

## Test Categories

Integration tests are marked with traits for selective execution:

```csharp
[Trait("Category", "Integration")]
[Trait("Layer", "DataAccess")]
public class DatabaseTests { }

[Trait("Category", "Integration")]
[Trait("Layer", "FileSystem")]
public class FileSystemTests { }
```

### Running Specific Categories

```bash
# Run all integration tests
dotnet test --filter "Category=Integration"

# Run only database tests
dotnet test --filter "Layer=DataAccess"

# Run only fast integration tests
dotnet test --filter "Category=Integration&Speed!=Slow"
```

## Best Practices Demonstrated

1. ✅ Tests clean up resources (IDisposable pattern)
2. ✅ Tests are independent and can run in any order
3. ✅ Use realistic test data
4. ✅ Test both success and failure scenarios
5. ✅ Proper exception handling
6. ✅ Appropriate use of async/await
7. ✅ Strategic mocking (only external dependencies)
8. ✅ Clear test naming and organization

## Test Data Management

### Builder Pattern

```csharp
var package = new PackageBuilder()
    .WithId("test.package")
    .WithName("Test Package")
    .WithVersion("1.0.0")
    .Build();
```

### Object Mother Pattern

```csharp
var packages = TestData.CreatePackageList(10);
var defaultPackage = TestData.CreateDefaultPackage();
```

### Test Fixtures

```csharp
[Collection("Database collection")]
public class MyTests
{
    private readonly DatabaseFixture _fixture;
    
    public MyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

## Related Documentation

- [Integration Testing Guide](../../../docs/testing/integration-testing-guide.md)
- [Unit Testing Guide](../../../docs/testing/unit-testing-guide.md)

## Additional Resources

- [.NET Integration Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Entity Framework In-Memory Database](https://docs.microsoft.com/en-us/ef/core/testing/)
- [Moq Documentation](https://github.com/moq/moq4)
