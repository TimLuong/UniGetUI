# UniGetUI Logging and Observability Examples

This directory contains comprehensive examples demonstrating best practices for logging, monitoring, and diagnostics in Windows applications.

## Overview

These examples complement the documentation in `/docs/observability/` and provide practical, runnable implementations of the patterns and practices described in the standards.

## Directory Structure

```
examples/
└── logging/
    └── structured-logging/
        ├── README.md                           # Overview of all examples
        ├── SerilogExample/                     # Serilog structured logging
        ├── NLogExample/                        # NLog with XML configuration
        ├── CorrelationExample/                 # Correlation ID tracking
        ├── ExceptionHandlingExample/           # Exception patterns
        ├── HealthCheckExample/                 # Health monitoring
        ├── MetricsExample/                     # Metrics collection
        └── DistributedTracingExample/          # OpenTelemetry tracing
```

## Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Windows, Linux, or macOS

### Running an Example

Navigate to any example directory and run:

```bash
cd logging/structured-logging/SerilogExample
dotnet restore
dotnet build
dotnet run
```

## Examples Summary

### 1. SerilogExample
**Focus**: Modern structured logging with Serilog

**Demonstrates**:
- Structured properties instead of string interpolation
- Multiple sinks (console, file)
- Log enrichment (thread ID, machine name)
- Log context for automatic property inclusion
- Configuration from appsettings.json

**Use When**: Building modern .NET applications that need rich, queryable logs

**Key Files**:
- `Program.cs` - Main demonstration code
- `appsettings.json` - Serilog configuration
- `README.md` - Detailed usage guide

---

### 2. NLogExample
**Focus**: Enterprise logging with NLog

**Demonstrates**:
- XML-based configuration
- Multiple targets with different log levels
- Archive policies
- Layout customization

**Use When**: Working with enterprise systems or needing XML configuration

**Key Files**:
- `Program.cs` - Basic NLog usage
- `NLog.config` - XML configuration
- `README.md` - Configuration guide

---

### 3. CorrelationExample
**Focus**: Request tracking across operations

**Demonstrates**:
- AsyncLocal-based correlation context
- Activity-based tracking (System.Diagnostics)
- LogContext integration
- Concurrent operation isolation

**Use When**: Need to track related operations or debug distributed systems

**Key Files**:
- `Program.cs` - Multiple correlation approaches
- `README.md` - Pattern explanations

---

### 4. ExceptionHandlingExample
**Focus**: Proper exception logging patterns

**Demonstrates**:
- Log and rethrow pattern
- Log and wrap pattern
- Log and handle pattern
- Global exception handlers
- Structured exception data

**Use When**: Establishing exception handling standards

**Key Files**:
- `Program.cs` - Exception patterns
- `README.md` - Best practices guide

---

### 5. HealthCheckExample
**Focus**: Application health monitoring

**Demonstrates**:
- Memory health checks
- File system checks
- Process resource checks
- External service checks
- Health aggregation

**Use When**: Building production-ready applications with health endpoints

**Key Files**:
- `Program.cs` - Complete health check system
- `README.md` - Integration patterns

---

### 6. MetricsExample
**Focus**: Performance and business metrics

**Demonstrates**:
- Counter metrics (cumulative values)
- Gauge metrics (point-in-time values)
- Histogram metrics (distributions)
- Timer metrics (duration tracking)
- Percentile calculations (P50, P95, P99)

**Use When**: Need to track KPIs or performance metrics

**Key Files**:
- `Program.cs` - Metrics infrastructure and usage
- `README.md` - Metrics types explained

---

### 7. DistributedTracingExample
**Focus**: End-to-end request tracing

**Demonstrates**:
- Activity creation and management
- Nested spans for operation hierarchy
- OpenTelemetry integration
- Error tracing
- Tag propagation

**Use When**: Debugging performance or tracking requests across services

**Key Files**:
- `Program.cs` - Tracing with OpenTelemetry
- `README.md` - Tracing patterns

## Learning Path

Recommended order for learning these concepts:

1. **Start with SerilogExample** - Learn structured logging basics
2. **Move to CorrelationExample** - Understand request tracking
3. **Study ExceptionHandlingExample** - Master error handling
4. **Explore HealthCheckExample** - Implement monitoring
5. **Review MetricsExample** - Add performance tracking
6. **Complete with DistributedTracingExample** - Enable distributed debugging

## Integration Guide

To integrate these patterns into UniGetUI or your application:

### 1. Replace Basic Logging
```csharp
// Before: Basic logging
Console.WriteLine($"Installing {packageId}");

// After: Structured logging (from SerilogExample)
Log.Information("Installing package {PackageId}", packageId);
```

### 2. Add Correlation
```csharp
// From CorrelationExample
using (CorrelationContext.BeginCorrelationScope())
{
    var correlationId = CorrelationContext.CorrelationId;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        // All operations automatically include correlation ID
    }
}
```

### 3. Implement Health Checks
```csharp
// From HealthCheckExample
var healthService = new HealthCheckService();
var report = await healthService.CheckHealthAsync();

if (report.Status != HealthStatus.Healthy)
{
    // Alert or take action
}
```

### 4. Collect Metrics
```csharp
// From MetricsExample
metrics.PackageInstalls.Increment();

using (metrics.InstallDuration.Time())
{
    await InstallPackageAsync();
}
```

## Documentation Links

- **[Logging Standards](../../docs/observability/logging-standards.md)** - Complete logging guidelines
- **[Monitoring Guide](../../docs/observability/monitoring-guide.md)** - Monitoring best practices
- **[Diagnostics Guide](../../docs/observability/diagnostics-guide.md)** - Troubleshooting workflows

## Contributing

When adding new examples:

1. **Follow existing structure** - Each example is self-contained
2. **Include README** - Explain what the example demonstrates
3. **Add comments** - Explain complex patterns in code
4. **Keep it simple** - Focus on one concept per example
5. **Make it runnable** - Ensure `dotnet run` works out of the box
6. **Test thoroughly** - Verify the example builds and runs

## Support

For questions or issues:
- Review the documentation in `/docs/observability/`
- Check individual example READMEs
- Review code comments in example files
- Open an issue in the repository

## License

These examples are part of the UniGetUI project and are provided under the same license.
