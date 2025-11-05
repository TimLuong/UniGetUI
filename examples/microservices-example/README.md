# Microservices Architecture Example

This example demonstrates microservices architecture patterns as they could be applied to a package management system like UniGetUI.

## Overview

This example shows how to build a distributed package management system using microservices architecture, including:

- **Service boundaries and contracts**
- **Inter-service communication (HTTP/REST and message queues)**
- **API Gateway pattern**
- **Service discovery**
- **Circuit breaker pattern**
- **Distributed logging and tracing**
- **Resilience patterns**

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     API Gateway                          │
│            (Routing, Auth, Rate Limiting)                │
└─────────────────────────────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
┌───────▼────────┐ ┌──────▼───────┐ ┌────────▼────────┐
│  Package       │ │  Installation │ │  Update         │
│  Discovery     │ │  Service      │ │  Service        │
│  Service       │ │               │ │                 │
└────────────────┘ └───────────────┘ └─────────────────┘
        │                  │                  │
        └──────────────────┼──────────────────┘
                           │
                    ┌──────▼───────┐
                    │  Message Bus │
                    │  (RabbitMQ)  │
                    └──────────────┘
```

## Services

### 1. API Gateway Service
**Port:** 5000  
**Responsibility:** Routes requests, handles authentication, rate limiting

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddReverseProxy()
        .LoadFromConfig(Configuration.GetSection("ReverseProxy"));
    
    services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("api", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
        });
    });
}
```

### 2. Package Discovery Service
**Port:** 5001  
**Responsibility:** Search and discover packages from various sources

**Endpoints:**
- `GET /api/packages/search?query={query}` - Search packages
- `GET /api/packages/{id}` - Get package details
- `GET /api/packages/sources` - List available sources

**Example:**
```csharp
[ApiController]
[Route("api/packages")]
public class PackageController : ControllerBase
{
    private readonly IPackageDiscoveryService _discoveryService;
    
    [HttpGet("search")]
    public async Task<ActionResult<List<PackageDto>>> Search(
        [FromQuery] string query,
        [FromQuery] string? source = null)
    {
        var packages = await _discoveryService.SearchPackagesAsync(query, source);
        return Ok(packages);
    }
}
```

### 3. Installation Service
**Port:** 5002  
**Responsibility:** Handle package installation and uninstallation

**Endpoints:**
- `POST /api/install` - Install a package
- `POST /api/uninstall` - Uninstall a package
- `GET /api/install/{id}/status` - Get installation status

**Example:**
```csharp
[ApiController]
[Route("api/install")]
public class InstallationController : ControllerBase
{
    private readonly IInstallationService _installService;
    private readonly IMessagePublisher _messagePublisher;
    
    [HttpPost]
    public async Task<ActionResult<InstallationResponse>> Install(
        [FromBody] InstallRequest request)
    {
        // Publish installation started event
        await _messagePublisher.PublishAsync(new InstallationStartedEvent
        {
            PackageId = request.PackageId,
            Timestamp = DateTime.UtcNow
        });
        
        // Perform installation
        var result = await _installService.InstallAsync(request);
        
        // Publish installation completed event
        await _messagePublisher.PublishAsync(new InstallationCompletedEvent
        {
            PackageId = request.PackageId,
            Success = result.Success,
            Timestamp = DateTime.UtcNow
        });
        
        return Ok(result);
    }
}
```

### 4. Update Service
**Port:** 5003  
**Responsibility:** Check for and manage package updates

**Endpoints:**
- `GET /api/updates` - Get available updates
- `POST /api/updates/check` - Trigger update check
- `POST /api/updates/install` - Install updates

## Communication Patterns

### Synchronous (HTTP/REST)
Used for:
- Request/response operations
- Real-time queries
- Client-facing APIs

```csharp
public class PackageHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ICircuitBreakerPolicy _circuitBreaker;
    
    public async Task<PackageDto> GetPackageAsync(string packageId)
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"/api/packages/{packageId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PackageDto>();
        });
    }
}
```

### Asynchronous (Message Bus)
Used for:
- Event notifications
- Background processing
- Service decoupling

```csharp
public class InstallationEventHandler : IMessageHandler<InstallationCompletedEvent>
{
    private readonly ILogger<InstallationEventHandler> _logger;
    
    public async Task HandleAsync(InstallationCompletedEvent evt)
    {
        _logger.LogInformation(
            "Package {PackageId} installation completed. Success: {Success}",
            evt.PackageId,
            evt.Success);
        
        // Update local cache, send notifications, etc.
    }
}
```

## Resilience Patterns

### Circuit Breaker
Prevents cascading failures by stopping requests to failing services:

```csharp
public class CircuitBreakerPolicy : ICircuitBreakerPolicy
{
    private readonly ICircuitBreaker _breaker;
    
    public CircuitBreakerPolicy()
    {
        _breaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    // Log circuit opened
                },
                onReset: () =>
                {
                    // Log circuit reset
                });
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        return await _breaker.ExecuteAsync(action);
    }
}
```

### Retry Policy
Automatically retries failed requests with exponential backoff:

```csharp
public class RetryPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                });
    }
}
```

### Timeout Policy
Prevents hanging requests:

```csharp
public class TimeoutPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    {
        return Policy
            .TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(10),
                onTimeoutAsync: async (context, timespan, task) =>
                {
                    // Log timeout
                });
    }
}
```

## Service Discovery

Using Consul for service registration and discovery:

```csharp
public class ConsulServiceRegistry : IServiceRegistry
{
    private readonly IConsulClient _consulClient;
    
    public async Task RegisterAsync(ServiceRegistration registration)
    {
        var consulReg = new AgentServiceRegistration
        {
            ID = registration.Id,
            Name = registration.ServiceName,
            Address = registration.Address,
            Port = registration.Port,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{registration.Address}:{registration.Port}/health",
                Interval = TimeSpan.FromSeconds(10)
            }
        };
        
        await _consulClient.Agent.ServiceRegister(consulReg);
    }
    
    public async Task<List<ServiceInstance>> DiscoverAsync(string serviceName)
    {
        var services = await _consulClient.Health.Service(serviceName, tag: null, passingOnly: true);
        return services.Response.Select(s => new ServiceInstance
        {
            Address = s.Service.Address,
            Port = s.Service.Port
        }).ToList();
    }
}
```

## Distributed Logging

Using Serilog with centralized logging:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.WithProperty("ServiceName", "PackageDiscoveryService")
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "microservices-{0:yyyy.MM.dd}"
                });
        });
```

## Distributed Tracing

Using OpenTelemetry for distributed tracing:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOpenTelemetryTracing(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("PackageDiscoveryService")
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "jaeger";
                options.AgentPort = 6831;
            });
    });
}
```

## Configuration

### appsettings.json
```json
{
  "ServiceDiscovery": {
    "ConsulUrl": "http://consul:8500"
  },
  "MessageBus": {
    "RabbitMQ": {
      "Host": "rabbitmq",
      "Port": 5672,
      "Username": "guest",
      "Password": "guest"
    }
  },
  "CircuitBreaker": {
    "FailureThreshold": 3,
    "DurationOfBreak": "00:00:30"
  }
}
```

## Running the Example

### Using Docker Compose

```yaml
version: '3.8'

services:
  api-gateway:
    build: ./ApiGateway
    ports:
      - "5000:80"
    depends_on:
      - consul
      - rabbitmq

  package-discovery:
    build: ./PackageDiscovery
    ports:
      - "5001:80"
    depends_on:
      - consul
      - rabbitmq

  installation-service:
    build: ./Installation
    ports:
      - "5002:80"
    depends_on:
      - consul
      - rabbitmq

  consul:
    image: consul:latest
    ports:
      - "8500:8500"

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:7.15.0
    ports:
      - "9200:9200"

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
```

Run with:
```bash
docker-compose up
```

## Testing

### Integration Tests
```csharp
public class PackageDiscoveryIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    
    [Fact]
    public async Task SearchPackages_ReturnsResults()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/packages/search?query=test");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var packages = await response.Content.ReadFromJsonAsync<List<PackageDto>>();
        Assert.NotEmpty(packages);
    }
}
```

## Best Practices

1. **Service Boundaries**: Keep services focused and independent
2. **API Versioning**: Use versioned APIs (/api/v1/)
3. **Health Checks**: Implement health endpoints for monitoring
4. **Graceful Degradation**: Handle service failures gracefully
5. **Observability**: Implement logging, metrics, and tracing
6. **Security**: Use authentication and authorization at the gateway
7. **Documentation**: Use Swagger/OpenAPI for API documentation

## Further Reading

- [Microservices.io Patterns](https://microservices.io/patterns/)
- [.NET Microservices Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Service Mesh](https://servicemesh.io/)

## Building This Example

```bash
cd microservices-example
docker-compose build
docker-compose up
```

## License

Part of the UniGetUI project examples.
