# Error Handling Examples

This directory contains comprehensive examples demonstrating error handling and exception management patterns for UniGetUI.

## Overview

These examples illustrate the concepts and patterns documented in:
- [Exception Strategy](../docs/error-handling/exception-strategy.md)
- [Resilience Patterns](../docs/error-handling/resilience-patterns.md)

## Examples

### 01-CustomExceptionHierarchy.cs

Demonstrates the custom exception hierarchy for UniGetUI including:
- Base `UniGetUIException` class with user-friendly messaging
- Package manager specific exceptions
- Network and connectivity exceptions
- Configuration and validation exceptions
- Usage examples showing how to throw and handle custom exceptions

**Key Concepts:**
- Exception hierarchy design
- User-friendly vs technical error messages
- Error codes for tracking
- Conditional telemetry reporting

### 02-GlobalExceptionHandling.cs

Shows how to implement application-wide exception handling:
- Initializing global exception handlers
- Handling UI thread exceptions
- Handling background thread exceptions
- Handling unobserved task exceptions
- Emergency cleanup procedures
- Error dialog display

**Key Concepts:**
- Application domain exception handling
- Dispatcher unhandled exception handling
- Task scheduler unobserved exception handling
- Graceful degradation on handler failure

### 03-RetryAndCircuitBreaker.cs

Demonstrates resilience patterns using Polly library:
- Basic retry with fixed delay
- Exponential backoff retry
- Retry with jitter
- Conditional retry based on conditions
- Basic circuit breaker
- Advanced circuit breaker with failure rate
- Per-service circuit breakers
- Timeout patterns (optimistic and pessimistic)
- Combining multiple policies (Policy Wrap)

**Key Concepts:**
- Transient failure handling
- Cascading failure prevention
- Timeout strategies
- Policy composition
- Production-ready implementations

**Prerequisites:**
```bash
dotnet add package Polly
```

### 04-ErrorRecoveryStrategies.cs

Illustrates various error recovery and graceful degradation patterns:
- Multi-level fallback strategies
- Partial success patterns
- Degraded mode operations
- Safe execution wrappers
- Stateful service with recovery
- State management during failures

**Key Concepts:**
- Fallback hierarchies (online → cache → default)
- Maintaining functionality during failures
- Automatic recovery from transient issues
- State tracking and consecutive failure handling

### 05-OfflineHandling.cs

Covers offline scenario handling and user-friendly error messages:
- Network connectivity monitoring
- Offline-aware service operations
- Cache-based fallbacks
- User-friendly error message construction
- Offline mode UI feedback
- Context-specific error messages

**Key Concepts:**
- Network availability detection
- Cache-first strategies when offline
- Clear user communication
- Graceful handling of network transitions
- Error message best practices

## Running the Examples

Each example file contains a `Main` method or demo class that can be run independently:

### Visual Studio / Rider
1. Open the solution in your IDE
2. Set the example file as the startup file or use the test runner
3. Run the program

### Command Line
```bash
# Compile the example
csc /r:System.Net.Http.dll /r:Polly.dll ExampleFileName.cs

# Run the compiled example
ExampleFileName.exe
```

### Using .NET Interactive Notebooks
You can also run these examples in Jupyter notebooks with .NET Interactive:
```bash
dotnet tool install -g Microsoft.dotnet-interactive
dotnet interactive jupyter install
```

## Integration into Your Project

### 1. Custom Exceptions

Copy the exception classes from `01-CustomExceptionHierarchy.cs` into your project:

```csharp
// In your project: UniGetUI.Core.Exceptions/UniGetUIException.cs
namespace UniGetUI.Core.Exceptions;

public class UniGetUIException : Exception
{
    // ... implementation from example
}
```

### 2. Global Exception Handler

Add global exception handling to your `App.xaml.cs`:

```csharp
public partial class App : Application
{
    public App()
    {
        GlobalExceptionHandler.Initialize();
    }
}
```

### 3. Resilience Patterns

Add Polly to your project and configure policies:

```xml
<PackageReference Include="Polly" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
```

```csharp
// Configure in Startup.cs or Program.cs
services.AddHttpClient("PackageManager")
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

### 4. Offline Handling

Integrate network monitoring into your services:

```csharp
public class PackageService
{
    private readonly NetworkMonitor _networkMonitor;
    
    public PackageService()
    {
        _networkMonitor = new NetworkMonitor();
        _networkMonitor.ConnectivityChanged += OnConnectivityChanged;
    }
}
```

## Best Practices Demonstrated

### 1. Exception Design
- ✅ Custom exception hierarchy with domain-specific context
- ✅ Separation of technical and user-friendly messages
- ✅ Error codes for tracking and documentation
- ✅ Conditional telemetry reporting (don't report user errors)

### 2. Error Handling
- ✅ Global exception handlers for different thread types
- ✅ Graceful degradation on handler failure
- ✅ Emergency cleanup procedures
- ✅ Appropriate use of try-catch blocks

### 3. Resilience
- ✅ Retry transient failures with exponential backoff
- ✅ Circuit breakers to prevent cascading failures
- ✅ Timeouts to prevent indefinite waiting
- ✅ Policy composition for comprehensive protection

### 4. Recovery
- ✅ Multi-level fallback strategies
- ✅ Cache-based recovery
- ✅ Partial success handling
- ✅ State tracking and automatic recovery

### 5. User Experience
- ✅ Clear, actionable error messages
- ✅ Offline mode with graceful degradation
- ✅ Visual feedback for connectivity status
- ✅ Contextual help and suggestions

## Common Patterns

### Pattern 1: Try-Cache-Default
```csharp
try
{
    return await FetchFromOnline();
}
catch (NetworkException)
{
    var cached = TryCache();
    return cached ?? GetDefault();
}
```

### Pattern 2: Retry with Circuit Breaker
```csharp
var policy = Policy.WrapAsync(
    circuitBreaker,
    retryPolicy,
    timeoutPolicy
);
```

### Pattern 3: Offline-Aware Operation
```csharp
if (!networkMonitor.IsOnline)
{
    return ResultFromCache();
}
return await FetchOnline();
```

### Pattern 4: User-Friendly Errors
```csharp
var message = new ErrorMessageBuilder()
    .ForOperation("install")
    .OnTarget($"package '{packageId}'")
    .Because("network is unavailable")
    .WithSuggestion("Please check your connection")
    .Build();
```

## Additional Resources

- [Exception Strategy Documentation](../docs/error-handling/exception-strategy.md)
- [Resilience Patterns Documentation](../docs/error-handling/resilience-patterns.md)
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [.NET Exception Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

## Contributing

When adding new error handling examples:

1. Follow the existing file naming convention (`##-DescriptiveName.cs`)
2. Include comprehensive inline comments
3. Provide usage examples with output
4. Update this README with the new example
5. Ensure examples are self-contained and runnable
6. Follow the coding standards in the main documentation

## Questions or Issues?

If you have questions about these examples or encounter issues:
1. Check the documentation in `/docs/error-handling/`
2. Review the inline comments in the example files
3. Open an issue on the GitHub repository
