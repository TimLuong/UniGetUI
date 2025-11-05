# WPF Reference Application

This is a comprehensive WPF (Windows Presentation Foundation) reference application demonstrating all the design patterns and best practices documented in the UniGetUI codebase.

## Overview

This reference application showcases:
- Factory Pattern implementation
- Singleton Pattern (static class variant)
- Observer Pattern with custom events
- Strategy Pattern for pluggable behaviors
- Task Recycler pattern for CPU optimization
- MVVM architecture
- Async/Await best practices
- Error handling patterns
- Modern C# features (C# 12)

## Project Structure

```
wpf-reference-app/
├── App.xaml                    # Application definition
├── App.xaml.cs                 # Application code-behind
├── MainWindow.xaml             # Main window XAML
├── MainWindow.xaml.cs          # Main window code-behind
├── ViewModels/
│   ├── MainViewModel.cs        # Main ViewModel with INotifyPropertyChanged
│   └── PackageViewModel.cs     # Package item ViewModel
├── Models/
│   ├── Package.cs              # Package domain model
│   └── PackageSource.cs        # Package source model
├── Services/
│   ├── IPackageService.cs      # Package service interface (Strategy Pattern)
│   ├── PackageServiceFactory.cs # Factory for creating package services
│   ├── WinGetService.cs        # WinGet implementation
│   ├── ScoopService.cs         # Scoop implementation
│   └── TaskRecycler.cs         # Task recycler for CPU optimization
├── Utilities/
│   ├── ObservableQueue.cs      # Observable queue implementation
│   └── Logger.cs               # Logging utility
└── README.md                   # This file
```

## Key Features Demonstrated

### 1. Factory Pattern
The `PackageServiceFactory` creates instances of package services while maintaining a cache of created instances:
```csharp
public class PackageServiceFactory
{
    private readonly ConcurrentDictionary<string, IPackageService> _services = new();
    
    public IPackageService GetService(string serviceType)
    {
        if (_services.TryGetValue(serviceType, out IPackageService? service))
        {
            return service;
        }
        // Create and cache new service
    }
}
```

### 2. Singleton Pattern
Static classes provide single point of access to shared functionality:
```csharp
public static class TaskRecycler<T>
{
    private static readonly ConcurrentDictionary<int, Task<T>> _tasks = new();
    // Single instance across application
}
```

### 3. Observer Pattern
Custom events notify subscribers of state changes:
```csharp
public class ObservableQueue<T> : Queue<T>
{
    public event EventHandler<EventArgs>? ItemEnqueued;
    // Raises events when items are added
}
```

### 4. Strategy Pattern
Different package services implement a common interface:
```csharp
public interface IPackageService
{
    Task<List<Package>> GetAvailablePackages();
    Task<bool> InstallPackage(Package package);
}
```

### 5. MVVM Architecture
- **Models**: Domain entities (Package, PackageSource)
- **Views**: XAML UI definitions
- **ViewModels**: Presentation logic with INotifyPropertyChanged

### 6. Task Recycler Pattern
Reduces CPU impact by reusing results from concurrent identical operations:
```csharp
var task1 = TaskRecycler<List<Package>>.RunOrAttachAsync(LoadPackages);
var task2 = TaskRecycler<List<Package>>.RunOrAttachAsync(LoadPackages);
// task2 attaches to task1 if it's the same operation
```

## Building the Application

### Prerequisites
- .NET 8.0 SDK
- Windows 10 or 11
- Visual Studio 2022 (recommended) or .NET CLI

### Build Instructions

#### Using Visual Studio
1. Open `WpfReferenceApp.sln` in Visual Studio 2022
2. Select **Release** or **Debug** configuration
3. Build > Build Solution (or press `Ctrl+Shift+B`)
4. Press F5 to run

#### Using .NET CLI
```bash
# Restore dependencies
dotnet restore WpfReferenceApp.sln

# Build the application
dotnet build WpfReferenceApp.sln --configuration Release

# Run the application
dotnet run --project WpfReferenceApp/WpfReferenceApp.csproj
```

## Running the Application

After building, you can run the application:

1. Launch `WpfReferenceApp.exe` from the build output directory
2. The main window will display a list of available package managers
3. Click "Load Packages" to see packages from different sources
4. Observe how the Factory Pattern creates service instances
5. See the Task Recycler in action when loading the same data concurrently
6. Watch the ObservableQueue events fire when packages are queued

## Code Examples

### Factory Pattern Implementation
Located in `Services/PackageServiceFactory.cs`:
- Centralizes object creation logic
- Provides caching mechanism for reusing instances
- Thread-safe using ConcurrentDictionary

### Task Recycler Implementation
Located in `Services/TaskRecycler.cs`:
- Reduces CPU usage by avoiding duplicate work
- Provides optional caching of results
- Thread-safe concurrent operations

### Observable Queue Implementation
Located in `Utilities/ObservableQueue.cs`:
- Raises events when items are enqueued or dequeued
- Follows .NET event pattern conventions
- Useful for UI updates and logging

### MVVM Implementation
Located in `ViewModels/MainViewModel.cs`:
- Implements INotifyPropertyChanged
- Uses RelayCommand for button bindings
- Separates presentation logic from UI

## Best Practices Demonstrated

### 1. Naming Conventions
- camelCase for local variables and parameters
- PascalCase for public methods and properties
- `_camelCase` with underscore prefix for private fields
- `I` prefix for interfaces

### 2. Async/Await Standards
- Async methods suffixed with 'Async'
- ConfigureAwait(false) for library code
- Task<T> for async methods with return values

### 3. Error Handling
- Try-catch blocks around potentially failing operations
- Logging errors with detailed information
- Fail-safe methods returning safe defaults

### 4. Modern C# Features
- File-scoped namespaces (C# 10)
- Primary constructors (C# 12)
- Target-typed new (C# 9)
- Nullable reference types

### 5. Code Organization
- Single responsibility principle
- Helper classes for delegation
- Clear separation of concerns

## Testing

The application includes example test patterns:
- Unit tests for ViewModels
- Integration tests for Services
- Mock implementations for testing

See the test project for examples of testing WPF applications.

## Additional Resources

- [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [MVVM Pattern](https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [Async Programming](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Design Patterns in C#](https://refactoring.guru/design-patterns/csharp)

## License

This example is provided as part of the UniGetUI project documentation and follows the same license as the main project.
