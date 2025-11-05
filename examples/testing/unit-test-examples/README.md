# Unit Test Examples

This directory contains comprehensive examples of unit tests demonstrating best practices for testing .NET applications with xUnit.

## Examples Included

1. **BasicUnitTests.cs** - Simple unit tests with Facts and Theories
2. **AsyncTestingExamples.cs** - Testing async methods and tasks
3. **MockingExamples.cs** - Using Moq for mocking dependencies
4. **CollectionTestingExamples.cs** - Testing collections and lists
5. **ExceptionTestingExamples.cs** - Testing exception handling

## Running the Examples

```bash
cd examples/testing/unit-test-examples
dotnet test
```

## Key Concepts Demonstrated

### 1. AAA Pattern (Arrange-Act-Assert)

Every test follows this structure:
- **Arrange**: Set up test data and preconditions
- **Act**: Execute the code under test
- **Assert**: Verify expected outcomes

### 2. Test Naming

Tests use descriptive names: `MethodName_Scenario_ExpectedBehavior`

Examples:
- `Add_TwoPositiveNumbers_ReturnsSum`
- `Divide_ByZero_ThrowsException`
- `GetUser_WithInvalidId_ReturnsNull`

### 3. Facts vs Theories

- **[Fact]**: Single test case
- **[Theory]** with **[InlineData]**: Multiple test cases with different inputs

### 4. Mocking

Use Moq to create test doubles for dependencies, isolating the system under test.

### 5. Async Testing

Properly test async methods using `async Task` and `await`.

## Example Test Structure

```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(5, 3);
        
        // Assert
        Assert.Equal(8, result);
    }
    
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(5, 3, 8)]
    [InlineData(-2, 2, 0)]
    public void Add_VariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
```

## Best Practices Demonstrated

1. ✅ Tests are independent - can run in any order
2. ✅ Single assertion per test (or related assertions)
3. ✅ Descriptive test names
4. ✅ No logic in tests
5. ✅ Fast execution
6. ✅ Deterministic results
7. ✅ Tests one thing at a time
8. ✅ Proper resource cleanup (IDisposable)

## Related Documentation

- [Unit Testing Guide](../../../docs/testing/unit-testing-guide.md)
- [Integration Testing Guide](../../../docs/testing/integration-testing-guide.md)

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
