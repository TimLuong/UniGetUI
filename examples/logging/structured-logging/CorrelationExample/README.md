# Correlation ID Example

This example demonstrates comprehensive correlation ID implementation for tracking requests and operations across multiple components.

## Features Demonstrated

- **AsyncLocal Context**: Thread-safe correlation context propagation
- **Activity-Based Tracking**: Using System.Diagnostics.Activity for distributed tracing
- **LogContext Integration**: Automatic correlation ID inclusion in all log entries
- **Concurrent Operations**: Tracking multiple independent operations
- **Nested Operations**: Parent-child relationship tracking

## Running the Example

```bash
dotnet restore
dotnet run
```

## Output

The example creates logs in `logs/correlation-YYYY-MM-DD.txt` showing how correlation IDs enable tracking of:
- Complete operation lifecycles
- Related log entries across different methods
- Concurrent operations independently
- Parent-child operation relationships

## Key Patterns

### AsyncLocal-Based Correlation
```csharp
using (CorrelationContext.BeginCorrelationScope())
{
    var correlationId = CorrelationContext.CorrelationId;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        // All logs in this scope have the same correlation ID
        Log.Information("Operation started");
        await DoWork();
        Log.Information("Operation completed");
    }
}
```

### Activity-Based Correlation
```csharp
using var activity = ActivitySource.StartActivity("OperationName");
if (activity != null)
{
    var correlationId = activity.Id;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        // Logs include activity ID for distributed tracing
        await DoWork();
    }
}
```

### LogContext Correlation
```csharp
var correlationId = Guid.NewGuid().ToString();
using (LogContext.PushProperty("CorrelationId", correlationId))
{
    // All logs automatically include CorrelationId
    Log.Information("Processing request");
}
```

## Use Cases

### Troubleshooting Failed Operations
When an operation fails, use the correlation ID to find all related log entries:
```
grep "abc-123-def" logs/correlation-2024-01-15.txt
```

### Performance Analysis
Track the complete lifecycle of an operation to identify bottlenecks:
```
[10:30:45 INF] [abc-123-def] Starting package installation
[10:30:46 DBG] [abc-123-def] Downloading package
[10:30:48 INF] [abc-123-def] Package downloaded successfully
[10:30:48 DBG] [abc-123-def] Installing package
[10:30:52 INF] [abc-123-def] Package installed successfully
```

### Concurrent Operation Isolation
Different operations have different correlation IDs, making it easy to separate interleaved logs:
```
[10:30:45 INF] [abc-123] Starting operation 1
[10:30:45 INF] [xyz-456] Starting operation 2
[10:30:46 DBG] [abc-123] Operation 1 step 1
[10:30:46 DBG] [xyz-456] Operation 2 step 1
[10:30:47 INF] [abc-123] Operation 1 completed
[10:30:48 INF] [xyz-456] Operation 2 completed
```

## Best Practices

1. ✅ Generate correlation IDs at operation boundaries
2. ✅ Propagate correlation IDs through all layers
3. ✅ Include correlation IDs in all log entries
4. ✅ Use consistent format (GUID recommended)
5. ✅ Pass correlation IDs to external services
6. ✅ Include correlation IDs in error messages
7. ✅ Store correlation IDs with operation results

## Integration Tips

To add correlation to an existing application:

1. **Add CorrelationContext class** to your project
2. **Wrap entry points** with `BeginCorrelationScope()`
3. **Include in logs** using `LogContext.PushProperty()`
4. **Propagate to external calls** via headers or context
5. **Return to callers** in responses for client-side tracking

## See Also

- [Logging Standards Documentation](../../../../docs/observability/logging-standards.md)
- [Diagnostics Guide](../../../../docs/observability/diagnostics-guide.md)
