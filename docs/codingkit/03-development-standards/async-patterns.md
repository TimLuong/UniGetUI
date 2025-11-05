### Async/Await Standards

#### Async Method Naming
```csharp
// Suffix async methods with 'Async'
public async Task DownloadUpdatedLanguageFile(string langKey)
public async Task<string> GetUserDataAsync()
```

#### ConfigureAwait Usage
```csharp
// Use ConfigureAwait(false) for library code to avoid deadlocks
await task.ConfigureAwait(false);
ReturnT result = await task.ConfigureAwait(false);
```

#### Task Return Types
```csharp
// Use Task for void-like async methods
public async Task RefreshPackageIndexes() { }

// Use Task<T> for async methods with return values
public async Task<string> GetDataAsync() { }
```

