# API Integration Patterns

This document provides comprehensive guidance on API integration patterns, including architectural patterns, communication protocols, and real-world implementation examples from the UniGetUI codebase.

## Table of Contents

- [Integration Architecture Patterns](#integration-architecture-patterns)
- [Communication Patterns](#communication-patterns)
- [GraphQL Implementation Guidelines](#graphql-implementation-guidelines)
- [WebSocket and Real-Time Communication](#websocket-and-real-time-communication)
- [Message Queue Patterns](#message-queue-patterns)
- [Service Integration Patterns](#service-integration-patterns)
- [Error Handling Patterns](#error-handling-patterns)
- [Retry and Circuit Breaker Patterns](#retry-and-circuit-breaker-patterns)
- [API Gateway Pattern](#api-gateway-pattern)
- [Backend for Frontend (BFF) Pattern](#backend-for-frontend-bff-pattern)
- [Testing Integration Patterns](#testing-integration-patterns)

---

## Integration Architecture Patterns

### 1. Point-to-Point Integration

Direct communication between two systems.

**When to Use:**
- Simple integrations
- Few systems involved
- Low complexity requirements

**Example from UniGetUI:**
```csharp
// Direct integration with Crates.io API
public class CratesIOClient
{
    public const string ApiUrl = "https://crates.io/api/v1";

    public static Tuple<Uri, CargoManifest> GetManifest(string packageId)
    {
        var manifestUrl = new Uri($"{ApiUrl}/crates/{packageId}");
        var manifest = Fetch<CargoManifest>(manifestUrl);
        return Tuple.Create(manifestUrl, manifest);
    }
}
```

**Advantages:**
- Simple implementation
- Low latency
- Easy to understand

**Disadvantages:**
- Tight coupling
- Difficult to scale with many integrations
- No central management

### 2. Hub-and-Spoke Pattern

Central hub mediates communication between systems.

**Architecture:**
```
┌─────────────┐
│  Service A  │──┐
└─────────────┘  │
                 │   ┌─────────────┐
┌─────────────┐  ├──▶│   Hub/ESB   │
│  Service B  │──┤   └─────────────┘
└─────────────┘  │
                 │
┌─────────────┐  │
│  Service C  │──┘
└─────────────┘
```

**When to Use:**
- Multiple services need to communicate
- Need centralized monitoring and management
- Complex routing requirements

**Example Implementation:**
```csharp
public class IntegrationHub
{
    private readonly Dictionary<string, IServiceClient> _clients;
    
    public async Task<TResponse> RouteRequest<TRequest, TResponse>(
        string serviceName, 
        TRequest request)
    {
        if (!_clients.TryGetValue(serviceName, out var client))
        {
            throw new ServiceNotFoundException(serviceName);
        }
        
        // Log request
        Logger.Info($"Routing request to {serviceName}");
        
        try
        {
            var response = await client.SendAsync<TRequest, TResponse>(request);
            Logger.Info($"Successfully received response from {serviceName}");
            return response;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to communicate with {serviceName}", ex);
            throw;
        }
    }
}
```

### 3. Microservices Integration

Services communicate through well-defined APIs.

**Architecture:**
```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Package    │────▶│  Download   │────▶│  Install    │
│  Service    │     │  Service    │     │  Service    │
└─────────────┘     └─────────────┘     └─────────────┘
       │                   │                   │
       └───────────────────┴───────────────────┘
                      │
              ┌───────────────┐
              │  Event Bus    │
              └───────────────┘
```

**Example from UniGetUI (Package Manager Architecture):**
```csharp
// Each package manager is a separate service with defined interface
public interface IPackageManager
{
    // Service boundaries
    public IPackageDetailsHelper DetailsHelper { get; }
    public IPackageOperationHelper OperationHelper { get; }
    public IMultiSourceHelper SourcesHelper { get; }
    
    // Service operations
    public IReadOnlyList<IPackage> FindPackages(string query);
    public IReadOnlyList<IPackage> GetAvailableUpdates();
    public IReadOnlyList<IPackage> GetInstalledPackages();
}
```

---

## Communication Patterns

### 1. Request-Response Pattern

Synchronous communication where client waits for response.

**When to Use:**
- Immediate response needed
- Simple query operations
- User-initiated actions

**Example:**
```csharp
public async Task<PackageDetails> GetPackageDetailsAsync(string packageId)
{
    HttpClient client = new(CoreTools.GenericHttpClientParameters);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(CoreData.UserAgentString);
    
    var response = await client.GetAsync($"{ApiUrl}/packages/{packageId}");
    response.EnsureSuccessStatusCode();
    
    var json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<PackageDetails>(json);
}
```

### 2. Fire-and-Forget Pattern

Asynchronous communication without waiting for response.

**When to Use:**
- Non-critical operations
- Telemetry and analytics
- Background tasks

**Example from UniGetUI (Telemetry):**
```csharp
public static async void ReportActivity()
{
    try
    {
        // Fire and forget - don't wait for response
        var request = new HttpRequestMessage(HttpMethod.Post, $"{HOST}/activity");
        request.Headers.Add("clientId", ID);
        request.Headers.Add("clientVersion", CoreData.VersionName);
        
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            Logger.Debug("[Telemetry] Call to /activity succeeded");
        }
    }
    catch (Exception ex)
    {
        // Fail silently for telemetry
        Logger.Error("[Telemetry] Hard crash when calling /activity");
        Logger.Error(ex);
    }
}
```

### 3. Publish-Subscribe Pattern

Publishers send messages to topics; subscribers receive from topics of interest.

**Architecture:**
```
┌────────────┐      ┌──────────────┐      ┌─────────────┐
│ Publisher  │─────▶│ Message Bus  │─────▶│ Subscriber  │
└────────────┘      │   (Topic)    │      └─────────────┘
                    └──────────────┘             │
                           │                      │
                           └──────────────────────┘
                                  │
                           ┌─────────────┐
                           │ Subscriber  │
                           └─────────────┘
```

**Example Implementation:**
```csharp
// Event-based communication
public class ObservableQueue<T> : Queue<T>
{
    public class EventArgs(T item)
    {
        public readonly T Item = item;
    }

    public event EventHandler<EventArgs>? ItemEnqueued;
    public event EventHandler<EventArgs>? ItemDequeued;

    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        ItemEnqueued?.Invoke(this, new EventArgs(item));
    }

    public new T Dequeue()
    {
        T item = base.Dequeue();
        ItemDequeued?.Invoke(this, new EventArgs(item));
        return item;
    }
}

// Usage
var queue = new ObservableQueue<Package>();

// Subscriber 1: Logger
queue.ItemEnqueued += (sender, args) => {
    Logger.Info($"Package enqueued: {args.Item.Name}");
};

// Subscriber 2: Telemetry
queue.ItemEnqueued += (sender, args) => {
    TelemetryHandler.ReportPackageOperation(args.Item);
};
```

### 4. Request-Callback Pattern

Client makes async request and provides callback for when response is ready.

**Example:**
```csharp
public class AsyncPackageManager
{
    public void GetPackageDetailsAsync(
        string packageId, 
        Action<PackageDetails> onSuccess,
        Action<Exception> onError)
    {
        Task.Run(async () =>
        {
            try
            {
                var details = await FetchPackageDetails(packageId);
                onSuccess(details);
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        });
    }
}

// Usage
manager.GetPackageDetailsAsync(
    "chrome",
    details => Console.WriteLine($"Got details: {details.Name}"),
    error => Console.WriteLine($"Error: {error.Message}")
);
```

### 5. Polling Pattern

Client periodically checks for updates.

**When to Use:**
- Server doesn't support push notifications
- Acceptable delay in updates
- Simple implementation needed

**Example from UniGetUI (Update Checker):**
```csharp
private async Task CheckForUpdates()
{
    while (true)
    {
        try
        {
            string UpdatesEndpoint = Settings.Get(Settings.K.EnableUniGetUIBeta) 
                ? BETA_ENDPOINT 
                : STABLE_ENDPOINT;
                
            HttpClient client = new();
            var response = await client.GetStringAsync(UpdatesEndpoint);
            
            // Process update information
            ProcessUpdateInfo(response);
            
            // Wait before next check (60 minutes on success, 10 on failure)
            await Task.Delay(TimeSpan.FromMinutes(updateSucceeded ? 60 : 10));
        }
        catch (Exception ex)
        {
            Logger.Error("Update check failed", ex);
        }
    }
}
```

---

## GraphQL Implementation Guidelines

GraphQL is an alternative to REST that provides flexible querying capabilities.

### When to Use GraphQL

**Good Use Cases:**
- Complex data requirements
- Multiple related entities
- Mobile apps with bandwidth constraints
- Reducing over-fetching/under-fetching

**Not Ideal For:**
- Simple CRUD operations
- File uploads/downloads
- Real-time streaming data
- Caching-heavy scenarios

### GraphQL Schema Design

**Example Schema:**
```graphql
type Package {
  id: ID!
  name: String!
  version: String!
  description: String
  author: Author!
  dependencies: [Dependency!]!
  downloads: Int!
  repository: String
  license: String
}

type Author {
  name: String!
  email: String
  packages: [Package!]!
}

type Dependency {
  package: Package!
  versionConstraint: String!
  isDevelopment: Boolean!
}

type Query {
  package(id: ID!): Package
  packages(
    search: String
    category: String
    limit: Int = 20
    offset: Int = 0
  ): [Package!]!
  
  searchPackages(
    query: String!
    filters: PackageFilters
  ): PackageConnection!
}

type Mutation {
  installPackage(id: ID!, version: String): InstallResult!
  updatePackage(id: ID!): InstallResult!
  uninstallPackage(id: ID!): UninstallResult!
}

type Subscription {
  packageInstallProgress(id: ID!): InstallProgress!
}

input PackageFilters {
  minDownloads: Int
  categories: [String!]
  licenses: [String!]
}

type PackageConnection {
  edges: [PackageEdge!]!
  pageInfo: PageInfo!
  totalCount: Int!
}

type PackageEdge {
  node: Package!
  cursor: String!
}

type PageInfo {
  hasNextPage: Boolean!
  hasPreviousPage: Boolean!
  startCursor: String
  endCursor: String
}
```

### GraphQL Implementation (C#)

**Using HotChocolate:**

```csharp
// Schema Types
public class Package
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
}

// Query Type
public class Query
{
    public async Task<Package> GetPackageAsync(
        [Service] IPackageRepository repository,
        string id)
    {
        return await repository.GetByIdAsync(id);
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Package> GetPackages(
        [Service] IPackageRepository repository)
    {
        return repository.GetQueryable();
    }
}

// Mutation Type
public class Mutation
{
    public async Task<InstallResult> InstallPackageAsync(
        [Service] IPackageManager manager,
        string id,
        string? version = null)
    {
        try
        {
            await manager.InstallPackageAsync(id, version);
            return new InstallResult 
            { 
                Success = true, 
                PackageId = id 
            };
        }
        catch (Exception ex)
        {
            return new InstallResult 
            { 
                Success = false, 
                Error = ex.Message 
            };
        }
    }
}

// Startup Configuration
services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();
```

### GraphQL Query Examples

**Simple Query:**
```graphql
query {
  package(id: "chrome") {
    name
    version
    description
  }
}
```

**Complex Query with Nested Relations:**
```graphql
query {
  package(id: "chrome") {
    name
    version
    author {
      name
      packages(limit: 5) {
        name
        downloads
      }
    }
    dependencies {
      package {
        name
        version
      }
      versionConstraint
    }
  }
}
```

**Query with Variables:**
```graphql
query GetPackages($search: String!, $limit: Int) {
  packages(search: $search, limit: $limit) {
    id
    name
    version
    downloads
  }
}

# Variables
{
  "search": "chrome",
  "limit": 10
}
```

**Mutation:**
```graphql
mutation {
  installPackage(id: "chrome", version: "120.0.0") {
    success
    packageId
    error
  }
}
```

### GraphQL Best Practices

1. **Design Schema First**: Define schema before implementation
2. **Use Descriptions**: Document types and fields
3. **Pagination**: Use cursor-based pagination for large datasets
4. **Error Handling**: Return errors in response, don't throw
5. **Batching**: Use DataLoader pattern to avoid N+1 queries
6. **Caching**: Implement caching at resolver level
7. **Rate Limiting**: Apply rate limits by complexity, not just requests

**DataLoader Example:**
```csharp
public class PackageDataLoader : BatchDataLoader<string, Package>
{
    private readonly IPackageRepository _repository;
    
    public PackageDataLoader(
        IPackageRepository repository,
        IBatchScheduler batchScheduler)
        : base(batchScheduler)
    {
        _repository = repository;
    }
    
    protected override async Task<IReadOnlyDictionary<string, Package>> 
        LoadBatchAsync(
            IReadOnlyList<string> keys, 
            CancellationToken cancellationToken)
    {
        // Batch load packages by IDs
        var packages = await _repository.GetByIdsAsync(keys, cancellationToken);
        return packages.ToDictionary(p => p.Id);
    }
}

// Usage in resolver
public async Task<Package> GetPackageAsync(
    [DataLoader] PackageDataLoader loader,
    string id)
{
    return await loader.LoadAsync(id);
}
```

---

## WebSocket and Real-Time Communication

WebSockets enable bidirectional, real-time communication between client and server.

### When to Use WebSockets

**Good Use Cases:**
- Real-time updates (progress, status)
- Chat applications
- Live dashboards
- Collaborative editing
- Gaming applications

**Example Use Case in UniGetUI:**
- Package installation progress
- Live update notifications
- Real-time download status

### WebSocket Implementation (Server - C#)

**Using ASP.NET Core:**

```csharp
public class PackageInstallWebSocketHandler
{
    private readonly List<WebSocket> _clients = new();
    
    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            _clients.Add(webSocket);
            
            await ReceiveMessagesAsync(webSocket);
            
            _clients.Remove(webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    
    private async Task ReceiveMessagesAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), 
                CancellationToken.None);
                
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection closed",
                    CancellationToken.None);
            }
            else
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessMessageAsync(message, webSocket);
            }
        }
    }
    
    public async Task BroadcastProgressAsync(InstallProgress progress)
    {
        var message = JsonSerializer.Serialize(progress);
        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);
        
        var tasks = _clients
            .Where(ws => ws.State == WebSocketState.Open)
            .Select(ws => ws.SendAsync(
                segment, 
                WebSocketMessageType.Text, 
                true, 
                CancellationToken.None));
                
        await Task.WhenAll(tasks);
    }
}

// Startup Configuration
app.UseWebSockets();

app.Map("/ws/install-progress", async context =>
{
    var handler = context.RequestServices
        .GetRequiredService<PackageInstallWebSocketHandler>();
    await handler.HandleWebSocketAsync(context);
});
```

### WebSocket Implementation (Client - JavaScript)

```javascript
class PackageInstallMonitor {
    constructor(packageId) {
        this.packageId = packageId;
        this.ws = null;
    }
    
    connect() {
        this.ws = new WebSocket('wss://api.example.com/ws/install-progress');
        
        this.ws.onopen = () => {
            console.log('WebSocket connected');
            this.subscribe(this.packageId);
        };
        
        this.ws.onmessage = (event) => {
            const progress = JSON.parse(event.data);
            this.handleProgress(progress);
        };
        
        this.ws.onerror = (error) => {
            console.error('WebSocket error:', error);
        };
        
        this.ws.onclose = () => {
            console.log('WebSocket closed');
            // Reconnect after delay
            setTimeout(() => this.connect(), 5000);
        };
    }
    
    subscribe(packageId) {
        this.ws.send(JSON.stringify({
            action: 'subscribe',
            packageId: packageId
        }));
    }
    
    handleProgress(progress) {
        console.log(`Progress: ${progress.percentage}%`);
        // Update UI with progress
        updateProgressBar(progress.percentage);
        updateStatusText(progress.message);
    }
    
    disconnect() {
        if (this.ws) {
            this.ws.close();
        }
    }
}

// Usage
const monitor = new PackageInstallMonitor('chrome');
monitor.connect();
```

### WebSocket Implementation (Client - C#)

```csharp
public class WebSocketClient
{
    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    
    public event EventHandler<InstallProgress> ProgressReceived;
    
    public async Task ConnectAsync(string url)
    {
        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        
        await _ws.ConnectAsync(new Uri(url), _cts.Token);
        
        _ = Task.Run(async () => await ReceiveMessagesAsync());
    }
    
    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];
        
        while (_ws.State == WebSocketState.Open)
        {
            var result = await _ws.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                _cts.Token);
                
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closed by server",
                    _cts.Token);
            }
            else
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var progress = JsonSerializer.Deserialize<InstallProgress>(message);
                ProgressReceived?.Invoke(this, progress);
            }
        }
    }
    
    public async Task SendAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await _ws.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            _cts.Token);
    }
    
    public async Task DisconnectAsync()
    {
        _cts.Cancel();
        if (_ws?.State == WebSocketState.Open)
        {
            await _ws.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Client closing",
                CancellationToken.None);
        }
        _ws?.Dispose();
    }
}

// Usage
var client = new WebSocketClient();
client.ProgressReceived += (sender, progress) => 
{
    Console.WriteLine($"Progress: {progress.Percentage}%");
};

await client.ConnectAsync("wss://api.example.com/ws/install-progress");
await client.SendAsync(JsonSerializer.Serialize(new 
{ 
    action = "subscribe", 
    packageId = "chrome" 
}));
```

### SignalR Alternative

For .NET applications, SignalR provides a higher-level abstraction over WebSockets:

```csharp
// Server Hub
public class InstallProgressHub : Hub
{
    public async Task SubscribeToPackage(string packageId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, packageId);
    }
    
    public async Task UnsubscribeFromPackage(string packageId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, packageId);
    }
}

// Broadcasting progress
public class PackageInstaller
{
    private readonly IHubContext<InstallProgressHub> _hubContext;
    
    public async Task InstallPackageAsync(string packageId)
    {
        // ... installation logic ...
        
        await _hubContext.Clients
            .Group(packageId)
            .SendAsync("ProgressUpdate", new 
            { 
                packageId, 
                percentage = 50,
                message = "Downloading..." 
            });
    }
}

// Client
var connection = new HubConnectionBuilder()
    .WithUrl("https://api.example.com/hubs/install-progress")
    .Build();

connection.On<InstallProgress>("ProgressUpdate", progress =>
{
    Console.WriteLine($"Progress: {progress.Percentage}%");
});

await connection.StartAsync();
await connection.InvokeAsync("SubscribeToPackage", "chrome");
```

---

## Message Queue Patterns

Message queues decouple systems and enable asynchronous processing.

### When to Use Message Queues

- High-volume processing
- Decoupling services
- Load leveling
- Guaranteed delivery
- Background jobs

### Queue-Based Load Leveling

```
┌──────────┐       ┌─────────┐       ┌───────────┐
│ Producer │──────▶│  Queue  │──────▶│ Consumer  │
└──────────┘       └─────────┘       └───────────┘
   Fast              Buffer             Slower
```

**Example Implementation:**

```csharp
public class PackageInstallQueue
{
    private readonly ObservableQueue<InstallRequest> _queue;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConcurrent;
    
    public PackageInstallQueue(int maxConcurrent = 3)
    {
        _queue = new ObservableQueue<InstallRequest>();
        _semaphore = new SemaphoreSlim(maxConcurrent);
        _maxConcurrent = maxConcurrent;
        
        StartProcessing();
    }
    
    public void Enqueue(InstallRequest request)
    {
        _queue.Enqueue(request);
        Logger.Info($"Queued install: {request.PackageId}");
    }
    
    private void StartProcessing()
    {
        _queue.ItemEnqueued += async (sender, args) =>
        {
            await _semaphore.WaitAsync();
            
            try
            {
                await ProcessInstallAsync(args.Item);
            }
            finally
            {
                _semaphore.Release();
            }
        };
    }
    
    private async Task ProcessInstallAsync(InstallRequest request)
    {
        try
        {
            Logger.Info($"Installing: {request.PackageId}");
            await _installer.InstallAsync(request.PackageId, request.Version);
            Logger.Info($"Installed: {request.PackageId}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to install {request.PackageId}", ex);
            // Could requeue or send to dead letter queue
        }
    }
}
```

### Priority Queue Pattern

Process high-priority items first:

```csharp
public class PriorityQueue<T>
{
    private readonly SortedDictionary<int, Queue<T>> _queues = new();
    
    public void Enqueue(T item, int priority)
    {
        if (!_queues.ContainsKey(priority))
        {
            _queues[priority] = new Queue<T>();
        }
        _queues[priority].Enqueue(item);
    }
    
    public T Dequeue()
    {
        // Get highest priority (lowest number)
        foreach (var kvp in _queues)
        {
            if (kvp.Value.Count > 0)
            {
                return kvp.Value.Dequeue();
            }
        }
        throw new InvalidOperationException("Queue is empty");
    }
}

// Usage
var queue = new PriorityQueue<InstallRequest>();
queue.Enqueue(criticalPackage, priority: 1);
queue.Enqueue(normalPackage, priority: 5);
queue.Enqueue(lowPriorityPackage, priority: 10);
```

---

## Service Integration Patterns

### 1. Adapter Pattern

Adapt external APIs to internal interfaces:

```csharp
// Internal interface
public interface IPackageSource
{
    Task<IEnumerable<Package>> SearchAsync(string query);
    Task<PackageDetails> GetDetailsAsync(string id);
}

// Adapter for external API
public class CratesIOAdapter : IPackageSource
{
    private readonly CratesIOClient _client;
    
    public async Task<IEnumerable<Package>> SearchAsync(string query)
    {
        // Adapt Crates.io response to internal Package model
        var results = await _client.SearchAsync(query);
        return results.Select(r => new Package
        {
            Id = r.crate.name,
            Name = r.crate.name,
            Version = r.crate.max_stable_version,
            Description = r.crate.description
        });
    }
    
    public async Task<PackageDetails> GetDetailsAsync(string id)
    {
        var (url, manifest) = _client.GetManifest(id);
        return new PackageDetails
        {
            Id = manifest.crate.name,
            Name = manifest.crate.name,
            Version = manifest.crate.newest_version,
            Description = manifest.crate.description,
            Homepage = manifest.crate.homepage,
            Repository = manifest.crate.repository
        };
    }
}
```

### 2. Facade Pattern

Simplify complex integrations:

```csharp
public class PackageManagerFacade
{
    private readonly IPackageManager _winget;
    private readonly IPackageManager _chocolatey;
    private readonly IPackageManager _scoop;
    
    public async Task<Package> FindPackageAnywhereAsync(string query)
    {
        // Search all package managers and return first result
        var tasks = new[]
        {
            _winget.FindPackagesAsync(query),
            _chocolatey.FindPackagesAsync(query),
            _scoop.FindPackagesAsync(query)
        };
        
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).FirstOrDefault();
    }
    
    public async Task InstallPackageAsync(string packageId, string source)
    {
        var manager = source switch
        {
            "winget" => _winget,
            "chocolatey" => _chocolatey,
            "scoop" => _scoop,
            _ => throw new ArgumentException($"Unknown source: {source}")
        };
        
        await manager.InstallPackageAsync(packageId);
    }
}
```

### 3. Aggregator Pattern

Combine data from multiple sources:

```csharp
public class PackageAggregator
{
    private readonly IEnumerable<IPackageSource> _sources;
    
    public async Task<AggregatedPackageInfo> GetAggregatedInfoAsync(string packageId)
    {
        var tasks = _sources.Select(s => s.GetDetailsAsync(packageId));
        var results = await Task.WhenAll(tasks);
        
        return new AggregatedPackageInfo
        {
            PackageId = packageId,
            AvailableIn = results.Select(r => r.Source).ToList(),
            LatestVersion = results.Max(r => Version.Parse(r.Version)),
            TotalDownloads = results.Sum(r => r.Downloads),
            AllVersions = results.SelectMany(r => r.Versions).Distinct()
        };
    }
}
```

---

## Error Handling Patterns

### 1. Retry Pattern

Automatically retry failed operations:

```csharp
public class RetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts = 3,
        TimeSpan? delay = null)
    {
        delay ??= TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                Logger.Warn($"Attempt {attempt} failed: {ex.Message}");
                await Task.Delay(delay.Value * attempt); // Exponential backoff
            }
        }
        
        // Last attempt without catching
        return await operation();
    }
}

// Usage
var result = await RetryPolicy.ExecuteAsync(
    async () => await httpClient.GetStringAsync(url),
    maxAttempts: 3
);
```

### 2. Circuit Breaker Pattern

Prevent cascading failures:

```csharp
public class CircuitBreaker
{
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;
    
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _state = CircuitState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }
        
        try
        {
            var result = await operation();
            
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
            }
            
            return result;
        }
        catch (Exception)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
            }
            
            throw;
        }
    }
}

enum CircuitState { Closed, Open, HalfOpen }
```

---

## Retry and Circuit Breaker Patterns

See [HTTP Client Best Practices](./http-client-best-practices.md) for detailed implementation examples.

---

## API Gateway Pattern

Centralized entry point for all API requests.

**Benefits:**
- Authentication/authorization
- Rate limiting
- Request routing
- Response caching
- Protocol translation
- Monitoring and logging

**Implementation Example:**

```csharp
public class ApiGateway
{
    private readonly IAuthenticationService _auth;
    private readonly IRateLimiter _rateLimiter;
    private readonly IServiceRouter _router;
    private readonly ICache _cache;
    
    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        // 1. Authentication
        if (!await _auth.AuthenticateAsync(context))
        {
            return new UnauthorizedResult();
        }
        
        // 2. Rate limiting
        if (!await _rateLimiter.AllowRequestAsync(context))
        {
            return new StatusCodeResult(429);
        }
        
        // 3. Check cache
        var cacheKey = GenerateCacheKey(context.Request);
        if (_cache.TryGet(cacheKey, out var cachedResponse))
        {
            return cachedResponse;
        }
        
        // 4. Route to appropriate service
        var service = _router.GetService(context.Request.Path);
        var response = await service.HandleAsync(context);
        
        // 5. Cache response
        _cache.Set(cacheKey, response);
        
        return response;
    }
}
```

---

## Backend for Frontend (BFF) Pattern

Separate backend for each frontend type (web, mobile, desktop).

**Example:**

```
┌─────────────┐        ┌──────────────┐
│   Web App   │───────▶│   Web BFF    │
└─────────────┘        └──────────────┘
                              │
┌─────────────┐        ┌──────┴───────┐        ┌─────────────┐
│ Mobile App  │───────▶│  Mobile BFF  │───────▶│  Services   │
└─────────────┘        └──────────────┘        └─────────────┘
                              │
┌─────────────┐        ┌──────┴───────┐
│Desktop App  │───────▶│ Desktop BFF  │
└─────────────┘        └──────────────┘
```

Each BFF optimizes for its client's needs.

---

## Testing Integration Patterns

### Integration Test Example

```csharp
[Fact]
public async Task TestCratesIOIntegration()
{
    // Arrange
    var client = new CratesIOClient();
    var packageId = "serde";
    
    // Act
    var (url, manifest) = client.GetManifest(packageId);
    
    // Assert
    Assert.NotNull(manifest);
    Assert.Equal(packageId, manifest.crate.name);
    Assert.NotEmpty(manifest.versions);
}
```

### Mock External Services

```csharp
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _responses;
    
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_responses.TryGetValue(request.RequestUri.ToString(), out var response))
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response)
            });
        }
        
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        });
    }
}

// Usage
var mockHandler = new MockHttpMessageHandler();
mockHandler.AddResponse(
    "https://crates.io/api/v1/crates/serde",
    "{\"crate\":{\"name\":\"serde\"}}"
);

var httpClient = new HttpClient(mockHandler);
```

---

## Related Documentation

- [REST API Guidelines](./rest-api-guidelines.md)
- [HTTP Client Best Practices](./http-client-best-practices.md)
- [External API Integrations](../codebase-analysis/05-integration/external-apis.md)
