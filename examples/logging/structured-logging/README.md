# Structured Logging Examples

This directory contains comprehensive examples demonstrating best practices for structured logging, monitoring, and diagnostics in Windows applications.

## Examples Overview

### 1. SerilogExample
Demonstrates structured logging using Serilog with:
- Console and file sinks
- Structured properties
- Log enrichment
- Multiple log levels
- Configuration from appsettings.json

**Key Features:**
- Structured logging with semantic properties
- Rolling file logging
- Console output with colored formatting
- Automatic property enrichment

### 2. NLogExample
Demonstrates logging using NLog with:
- XML-based configuration
- Multiple targets (console, file, database)
- Layout renderers
- Structured logging
- Conditional logging

**Key Features:**
- Flexible routing rules
- Archive and retention policies
- Async logging for performance
- Custom layout formatters

### 3. CorrelationExample
Demonstrates correlation ID implementation for:
- Request tracking across operations
- Activity-based correlation
- AsyncLocal context propagation
- Log context integration

**Key Features:**
- Automatic correlation ID generation
- Context propagation across async boundaries
- Integration with structured logging
- Trace visualization

### 4. ExceptionHandlingExample
Demonstrates exception handling patterns:
- Log and rethrow
- Log and wrap
- Log and handle
- Global exception handlers
- Structured exception logging

**Key Features:**
- Preserving stack traces
- Adding contextual information
- Exception sanitization
- Unhandled exception capture

### 5. HealthCheckExample
Demonstrates application health monitoring:
- Database health checks
- File system health checks
- External service health checks
- Health check aggregation
- Status reporting

**Key Features:**
- Component-level health status
- Degraded state detection
- Health check coordination
- Diagnostic data collection

### 6. MetricsExample
Demonstrates metrics collection:
- Counter metrics
- Gauge metrics
- Histogram metrics
- Timer metrics
- Custom business metrics

**Key Features:**
- Performance metrics
- Business KPIs
- Resource utilization tracking
- Percentile calculations

### 7. DistributedTracingExample
Demonstrates distributed tracing:
- Activity-based tracing
- Span creation and management
- Context propagation
- OpenTelemetry integration
- Trace analysis

**Key Features:**
- End-to-end request tracking
- Performance bottleneck identification
- Error correlation
- Service dependency mapping

## Getting Started

Each example is self-contained with its own project file and can be run independently.

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 or VS Code
- Basic understanding of C# and logging concepts

### Running an Example

1. Navigate to the example directory:
   ```bash
   cd examples/logging/structured-logging/SerilogExample
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the example:
   ```bash
   dotnet run
   ```

4. Review the output and generated log files in the `logs/` directory

## Integration with UniGetUI

These examples demonstrate patterns that can be integrated into the UniGetUI application:

1. **Replace basic logging** with structured logging from SerilogExample
2. **Add correlation IDs** using patterns from CorrelationExample
3. **Implement health checks** following HealthCheckExample
4. **Add metrics collection** using MetricsExample patterns
5. **Enable distributed tracing** with DistributedTracingExample

## Best Practices Demonstrated

- ✅ Structured logging with semantic properties
- ✅ Consistent log levels usage
- ✅ Correlation ID implementation
- ✅ Exception handling patterns
- ✅ Health check implementation
- ✅ Metrics collection strategies
- ✅ Distributed tracing setup
- ✅ Performance considerations
- ✅ Security and PII protection

## Additional Resources

- [Logging Standards Documentation](../../../docs/observability/logging-standards.md)
- [Monitoring Guide](../../../docs/observability/monitoring-guide.md)
- [Diagnostics Guide](../../../docs/observability/diagnostics-guide.md)

## Contributing

When adding new examples:
1. Follow the existing structure
2. Include comprehensive comments
3. Demonstrate real-world scenarios
4. Provide README with usage instructions
5. Ensure examples can run standalone

## License

These examples are part of the UniGetUI project and are provided under the same license.
