# Testing Strategy and Framework

## Overview

This document provides a high-level overview of the comprehensive testing strategy for UniGetUI, a Windows application built with .NET 8 and WinUI 3.

## Documentation Structure

### Core Testing Guides

1. **[Unit Testing Guide](./unit-testing-guide.md)**
   - xUnit framework fundamentals
   - Test structure and organization
   - AAA pattern (Arrange-Act-Assert)
   - Facts vs Theories
   - Async testing patterns
   - Mocking strategies with Moq
   - Code coverage requirements (80% minimum)
   - Best practices and common pitfalls

2. **[Integration Testing Guide](./integration-testing-guide.md)**
   - Component integration testing
   - Database integration with in-memory databases
   - File system testing
   - External service integration
   - Process execution testing
   - Test data management patterns
   - Mocking and stubbing strategies
   - Performance and load testing

3. **[UI Automation Guide](./ui-automation-guide.md)**
   - WinAppDriver for WinUI 3 automation
   - Appium Desktop integration
   - Element identification strategies
   - Page Object Model pattern
   - Accessibility testing
   - Visual regression testing
   - Performance testing
   - CI/CD integration

## Testing Framework Stack

### Core Framework
- **xUnit 2.9.3** - Primary testing framework
- **.NET 8.0** - Target framework
- **Coverlet** - Code coverage collection

### Recommended Tools
- **Moq 4.20.70** - Mocking framework (for integration/unit tests requiring mocks)
- **NSubstitute 5.1.0** - Alternative mocking framework
- **FluentAssertions** - Enhanced assertion library
- **WinAppDriver** - UI automation for Windows apps
- **Appium** - Cross-platform UI automation

### Coverage & Reporting
- **Coverlet.collector 6.0.4** - Code coverage
- **ReportGenerator** - HTML coverage reports
- **Microsoft.NET.Test.Sdk 17.14.1** - Test SDK

## Testing Pyramid

```
        /\
       /  \  UI Tests (Few - Critical paths only)
      /____\
     /      \
    /        \ Integration Tests (Some - Component interactions)
   /__________\
  /            \
 /              \ Unit Tests (Many - Individual components)
/________________\
```

### Test Distribution Guidelines

- **Unit Tests**: 70-80% of total tests
  - Fast execution (< 100ms per test)
  - No external dependencies
  - Test individual methods and classes
  - High coverage of business logic

- **Integration Tests**: 15-25% of total tests
  - Medium execution time (< 1 second per test)
  - Test component interactions
  - May use in-memory databases
  - Strategic mocking of external services

- **UI Tests**: 5-10% of total tests
  - Slower execution (seconds per test)
  - Test critical user workflows
  - End-to-end scenarios
  - User acceptance validation

## Code Coverage Requirements

### Minimum Coverage Standards

- **Overall Project**: 80% minimum code coverage
- **Core Libraries** (UniGetUI.Core.*): 85% minimum
- **Critical Path Code**: 90% minimum
- **New Code**: 80% minimum (enforced in PR reviews)
- **Package Managers**: 75% minimum (due to external dependencies)

### Coverage Exclusions

Exclude from coverage requirements:
- Auto-generated code
- Third-party libraries
- UI event handlers (covered by UI tests)
- Platform-specific interop code
- Logging infrastructure
- Configuration code

### Measuring Coverage

```bash
# Run tests with coverage
cd src
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html
```

## Test Organization

### Project Structure

```
src/
├── UniGetUI.Core.Classes/
│   └── TaskRecycler.cs
├── UniGetUI.Core.Classes.Tests/
│   ├── TaskRecyclerTests.cs
│   └── UniGetUI.Core.Classes.Tests.csproj
└── UniGetUI.sln

examples/testing/
├── unit-test-examples/
│   ├── BasicUnitTests.cs
│   ├── AsyncTestingExamples.cs
│   ├── MockingExamples.cs
│   └── README.md
├── integration-test-examples/
│   ├── FileSystemIntegrationTests.cs
│   └── README.md
└── ui-automation-examples/
    └── README.md
```

### Naming Conventions

- **Test Projects**: `{ProjectName}.Tests`
- **Test Files**: `{ClassUnderTest}Tests.cs`
- **Test Methods**: `{MethodName}_{Scenario}_{ExpectedBehavior}`

### Test Categories

Use traits to organize tests:

```csharp
[Trait("Category", "Unit")]
[Trait("Category", "Integration")]
[Trait("Category", "UI")]
[Trait("Speed", "Fast|Slow")]
[Trait("Layer", "DataAccess|Service|UI")]
```

## CI/CD Integration

### Automated Testing

Tests run automatically on:
- Every push to `main` branch
- Every pull request
- Manual workflow dispatch
- Nightly builds (for comprehensive test suites)

### GitHub Actions Workflows

- **`.github/workflows/dotnet-test.yml`** - Unit tests
- **Integration tests** - Run on specific triggers
- **UI tests** - Run nightly or on release branches

### Running Tests

```bash
# Run all tests
cd src
dotnet test

# Run specific test project
dotnet test UniGetUI.Core.Classes.Tests/

# Run with verbosity
dotnet test --verbosity normal

# Run specific category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Best Practices

### General Guidelines

1. ✅ **Write tests first** (TDD when possible)
2. ✅ **Keep tests simple** - No complex logic in tests
3. ✅ **Test one thing** - Single responsibility per test
4. ✅ **Make tests independent** - Can run in any order
5. ✅ **Use descriptive names** - Clear intent and behavior
6. ✅ **Follow AAA pattern** - Arrange, Act, Assert
7. ✅ **Clean up resources** - Use IDisposable
8. ✅ **Avoid test interdependencies** - Each test is isolated

### Code Quality

1. ✅ **Maintain 80%+ coverage** - Especially for core libraries
2. ✅ **Test edge cases** - Null, empty, boundary values
3. ✅ **Test error handling** - Exceptions and error states
4. ✅ **Mock external dependencies** - Only in integration tests
5. ✅ **Use realistic test data** - Representative of production
6. ✅ **Avoid hardcoded values** - Use constants or test data builders
7. ✅ **Review test failures** - Fix flaky tests immediately

### Performance

1. ✅ **Keep unit tests fast** - < 100ms per test
2. ✅ **Optimize integration tests** - < 1 second when possible
3. ✅ **Use parallel execution** - xUnit default behavior
4. ✅ **Avoid Thread.Sleep** - Use proper async/await
5. ✅ **Clean up test data** - Prevent resource leaks

## Mocking and Stubbing

### When to Mock

- External services (APIs, databases)
- Slow operations (network calls, file I/O)
- Non-deterministic behavior (time, random)
- Side effects (email, logging)

### When NOT to Mock

- Simple POCOs and DTOs
- The system under test
- Stable, fast internal dependencies

### Recommended Libraries

```xml
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="NSubstitute" Version="5.1.0" />
```

## Performance Testing

### Guidelines

- Measure execution time for critical operations
- Test with realistic data volumes
- Identify bottlenecks early
- Set performance benchmarks
- Monitor trends over time

### Load Testing

- Test concurrent operations
- Validate thread safety
- Test resource limits
- Measure memory usage
- Test scalability

## Resources

### Documentation
- [Unit Testing Guide](./unit-testing-guide.md)
- [Integration Testing Guide](./integration-testing-guide.md)
- [UI Automation Guide](./ui-automation-guide.md)

### Examples
- [Unit Test Examples](/examples/testing/unit-test-examples/)
- [Integration Test Examples](/examples/testing/integration-test-examples/)

### External Resources
- [xUnit Documentation](https://xunit.net/)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Moq Documentation](https://github.com/moq/moq4)
- [WinAppDriver](https://github.com/microsoft/WinAppDriver)

## Getting Started

1. **Review Documentation**
   - Read the [Unit Testing Guide](./unit-testing-guide.md)
   - Understand the testing patterns
   - Review example tests

2. **Set Up Your Environment**
   - Install .NET 8 SDK
   - Configure your IDE for testing
   - Install recommended extensions

3. **Write Your First Test**
   - Create a test project
   - Follow the naming conventions
   - Use the AAA pattern
   - Run and verify

4. **Integrate with CI/CD**
   - Tests run automatically
   - Monitor coverage reports
   - Fix failing tests promptly

## Support

For questions or issues:
- Review existing test examples
- Consult the testing guides
- Check xUnit documentation
- Ask in team discussions

## Continuous Improvement

This testing strategy is a living document and will evolve with:
- New testing tools and frameworks
- Lessons learned from the team
- Changes in project requirements
- Industry best practices

Regular reviews and updates ensure the testing strategy remains effective and relevant.
