# HTTP Client Patterns Examples

This directory contains practical examples demonstrating HTTP client best practices for C# .NET applications, based on patterns used in UniGetUI and industry standards.

## Examples Included

1. **BasicHttpClientUsage.cs** - Fundamental HttpClient patterns
2. **HttpClientFactoryExample.cs** - IHttpClientFactory dependency injection
3. **RetryPolicyExample.cs** - Implementing retry logic with Polly
4. **AuthenticationExample.cs** - Various authentication patterns
5. **ErrorHandlingExample.cs** - Comprehensive error handling
6. **StreamingExample.cs** - Handling large file downloads
7. **MockingExample.cs** - Unit testing HTTP clients

## Prerequisites

These examples require:
- .NET 8.0 or later
- NuGet packages:
  - `System.Net.Http`
  - `Microsoft.Extensions.Http`
  - `Polly` (for retry examples)
  - `Moq` (for testing examples)

## Installation

```bash
dotnet add package Microsoft.Extensions.Http
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
dotnet add package Moq
```

## Running the Examples

Each file is a self-contained example. To use them in your project:

1. Copy the relevant example file to your project
2. Install required NuGet packages
3. Adapt the code to your specific use case
4. Follow the comments in each example for guidance

## Key Patterns Demonstrated

### HttpClient Lifecycle Management
- ✅ Proper HttpClient reuse
- ✅ IHttpClientFactory integration
- ❌ Common anti-patterns to avoid

### Resilience Patterns
- Retry with exponential backoff
- Circuit breaker
- Timeout policies
- Combined policies

### Error Handling
- Status code handling
- Exception handling
- Rate limit detection
- Custom error types

### Authentication
- Bearer tokens
- API keys
- OAuth 2.0
- DelegatingHandler pattern

### Performance
- Connection pooling
- Compression
- Streaming large files
- HTTP/2 support

## Related Documentation

- [HTTP Client Best Practices](../../../docs/api/http-client-best-practices.md)
- [REST API Guidelines](../../../docs/api/rest-api-guidelines.md)
- [Integration Patterns](../../../docs/api/integration-patterns.md)

## Contributing

These examples are meant to be practical and educational. If you find improvements or have additional patterns to suggest, please contribute!
