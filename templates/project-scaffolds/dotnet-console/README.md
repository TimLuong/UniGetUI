# .NET Console Application Template

This template provides a complete scaffolding for a .NET console application following clean architecture principles.

## Structure

```
dotnet-console/
├── README.md (this file)
├── MyApp.sln
├── src/
│   ├── MyApp/
│   │   ├── MyApp.csproj
│   │   ├── Program.cs
│   │   ├── Commands/
│   │   │   └── ProcessCommand.cs
│   │   └── DependencyInjection.cs
│   ├── MyApp.Core/
│   │   ├── MyApp.Core.csproj
│   │   ├── Entities/
│   │   │   └── Item.cs
│   │   ├── Interfaces/
│   │   │   └── IItemRepository.cs
│   │   └── Services/
│   │       └── IItemService.cs
│   ├── MyApp.Application/
│   │   ├── MyApp.Application.csproj
│   │   ├── Services/
│   │   │   └── ItemService.cs
│   │   └── DTOs/
│   │       └── ItemDto.cs
│   └── MyApp.Infrastructure/
│       ├── MyApp.Infrastructure.csproj
│       ├── Repositories/
│       │   └── ItemRepository.cs
│       └── Data/
│           └── ApplicationDbContext.cs
├── tests/
│   ├── MyApp.Tests/
│   │   ├── MyApp.Tests.csproj
│   │   └── Services/
│   │       └── ItemServiceTests.cs
│   └── MyApp.Integration.Tests/
│       ├── MyApp.Integration.Tests.csproj
│       └── Repositories/
│           └── ItemRepositoryTests.cs
└── docs/
    └── README.md
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider

### Setup Instructions

1. Copy this template to your new project directory
2. Rename all instances of "MyApp" to your application name
3. Update namespace declarations
4. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
5. Build the solution:
   ```bash
   dotnet build
   ```
6. Run tests:
   ```bash
   dotnet test
   ```
7. Run the application:
   ```bash
   dotnet run --project src/MyApp/MyApp.csproj
   ```

### Quick Start Commands

```bash
# Create new solution from template
dotnet new console -n MyApp
cd MyApp

# Create class libraries for each layer
dotnet new classlib -n MyApp.Core -o src/MyApp.Core
dotnet new classlib -n MyApp.Application -o src/MyApp.Application
dotnet new classlib -n MyApp.Infrastructure -o src/MyApp.Infrastructure

# Create test projects
dotnet new xunit -n MyApp.Tests -o tests/MyApp.Tests
dotnet new xunit -n MyApp.Integration.Tests -o tests/MyApp.Integration.Tests

# Add project references
dotnet add src/MyApp/MyApp.csproj reference src/MyApp.Application/MyApp.Application.csproj
dotnet add src/MyApp.Application/MyApp.Application.csproj reference src/MyApp.Core/MyApp.Core.csproj
dotnet add src/MyApp.Infrastructure/MyApp.Infrastructure.csproj reference src/MyApp.Core/MyApp.Core.csproj

# Add projects to solution
dotnet sln add src/MyApp/MyApp.csproj
dotnet sln add src/MyApp.Core/MyApp.Core.csproj
dotnet sln add src/MyApp.Application/MyApp.Application.csproj
dotnet sln add src/MyApp.Infrastructure/MyApp.Infrastructure.csproj
dotnet sln add tests/MyApp.Tests/MyApp.Tests.csproj
dotnet sln add tests/MyApp.Integration.Tests/MyApp.Integration.Tests.csproj
```

## Project Files

### Directory.Build.props

Place this file in the root directory to share common properties across all projects:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

### src/MyApp/MyApp.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyApp.Application\MyApp.Application.csproj" />
    <ProjectReference Include="..\MyApp.Infrastructure\MyApp.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

### src/MyApp/Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp;
using MyApp.Commands;
using System.CommandLine;

// Build service provider
var services = new ServiceCollection();
DependencyInjection.ConfigureServices(services);
var serviceProvider = services.BuildServiceProvider();

// Create root command
var rootCommand = new RootCommand("MyApp - A sample console application");

// Add commands
var processCommand = new Command("process", "Process items")
{
    new Option<string>("--input", "Input file path") { IsRequired = true },
    new Option<string>("--output", "Output file path")
};

processCommand.SetHandler(async (string input, string output) =>
{
    var command = serviceProvider.GetRequiredService<ProcessCommand>();
    await command.ExecuteAsync(input, output);
},
processCommand.Options[0] as Option<string>,
processCommand.Options[1] as Option<string>);

rootCommand.AddCommand(processCommand);

// Execute
return await rootCommand.InvokeAsync(args);
```

### src/MyApp/DependencyInjection.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Application.Services;
using MyApp.Commands;
using MyApp.Core.Interfaces;
using MyApp.Core.Services;
using MyApp.Infrastructure.Repositories;

namespace MyApp;

public static class DependencyInjection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Core services
        services.AddScoped<IItemService, ItemService>();

        // Infrastructure
        services.AddScoped<IItemRepository, ItemRepository>();

        // Commands
        services.AddTransient<ProcessCommand>();
    }
}
```

### src/MyApp/Commands/ProcessCommand.cs

```csharp
using Microsoft.Extensions.Logging;
using MyApp.Core.Services;

namespace MyApp.Commands;

public class ProcessCommand
{
    private readonly IItemService _itemService;
    private readonly ILogger<ProcessCommand> _logger;

    public ProcessCommand(
        IItemService itemService,
        ILogger<ProcessCommand> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    public async Task ExecuteAsync(string inputPath, string? outputPath)
    {
        _logger.LogInformation("Processing items from {InputPath}", inputPath);

        try
        {
            var items = await _itemService.GetAllItemsAsync();
            
            _logger.LogInformation("Found {Count} items", items.Count());
            
            foreach (var item in items)
            {
                _logger.LogInformation("Item: {Name}", item.Name);
            }

            if (!string.IsNullOrEmpty(outputPath))
            {
                _logger.LogInformation("Output saved to {OutputPath}", outputPath);
            }

            _logger.LogInformation("Processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing items");
            throw;
        }
    }
}
```

### src/MyApp.Core/MyApp.Core.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>MyApp.Core</RootNamespace>
  </PropertyGroup>
</Project>
```

### src/MyApp.Core/Entities/Item.cs

```csharp
namespace MyApp.Core.Entities;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Item()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public Item(int id, string name, string description)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
    }
}
```

### src/MyApp.Core/Interfaces/IItemRepository.cs

```csharp
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id);
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item> AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
}
```

### src/MyApp.Core/Services/IItemService.cs

```csharp
using MyApp.Application.DTOs;

namespace MyApp.Core.Services;

public interface IItemService
{
    Task<IEnumerable<ItemDto>> GetAllItemsAsync();
    Task<ItemDto?> GetItemByIdAsync(int id);
    Task<ItemDto> CreateItemAsync(string name, string description);
    Task UpdateItemAsync(int id, string name, string description);
    Task DeleteItemAsync(int id);
}
```

### src/MyApp.Application/MyApp.Application.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>MyApp.Application</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
  </ItemGroup>
</Project>
```

### src/MyApp.Application/DTOs/ItemDto.cs

```csharp
namespace MyApp.Application.DTOs;

public record ItemDto(
    int Id,
    string Name,
    string Description,
    DateTime CreatedAt
);
```

### src/MyApp.Application/Services/ItemService.cs

```csharp
using MyApp.Application.DTOs;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Core.Services;

namespace MyApp.Application.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _repository;

    public ItemService(IItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ItemDto>> GetAllItemsAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<ItemDto?> GetItemByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<ItemDto> CreateItemAsync(string name, string description)
    {
        var item = new Item(0, name, description);
        var created = await _repository.AddAsync(item);
        return MapToDto(created);
    }

    public async Task UpdateItemAsync(int id, string name, string description)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
            throw new KeyNotFoundException($"Item with ID {id} not found");

        item.UpdateName(name);
        await _repository.UpdateAsync(item);
    }

    public async Task DeleteItemAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static ItemDto MapToDto(Item item)
    {
        return new ItemDto(
            item.Id,
            item.Name,
            item.Description,
            item.CreatedAt
        );
    }
}
```

### src/MyApp.Infrastructure/MyApp.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>MyApp.Infrastructure</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
  </ItemGroup>
</Project>
```

### src/MyApp.Infrastructure/Repositories/ItemRepository.cs

```csharp
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly List<Item> _items = new();
    private int _nextId = 1;

    public Task<Item?> GetByIdAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(item);
    }

    public Task<IEnumerable<Item>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Item>>(_items);
    }

    public Task<Item> AddAsync(Item item)
    {
        item.Id = _nextId++;
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task UpdateAsync(Item item)
    {
        var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem != null)
        {
            var index = _items.IndexOf(existingItem);
            _items[index] = item;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            _items.Remove(item);
        }
        return Task.CompletedTask;
    }
}
```

### tests/MyApp.Tests/MyApp.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <RootNamespace>MyApp.Tests</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\MyApp.Application\MyApp.Application.csproj" />
    <ProjectReference Include="..\..\src\MyApp.Core\MyApp.Core.csproj" />
  </ItemGroup>
</Project>
```

### tests/MyApp.Tests/Services/ItemServiceTests.cs

```csharp
using FluentAssertions;
using Moq;
using MyApp.Application.Services;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using Xunit;

namespace MyApp.Tests.Services;

public class ItemServiceTests
{
    private readonly Mock<IItemRepository> _mockRepository;
    private readonly ItemService _service;

    public ItemServiceTests()
    {
        _mockRepository = new Mock<IItemRepository>();
        _service = new ItemService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllItemsAsync_ReturnsAllItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item(1, "Item 1", "Description 1"),
            new Item(2, "Item 2", "Description 2")
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetAllItemsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Item 1");
    }

    [Fact]
    public async Task CreateItemAsync_CreatesNewItem()
    {
        // Arrange
        var newItem = new Item(1, "New Item", "Description");
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Item>())).ReturnsAsync(newItem);

        // Act
        var result = await _service.CreateItemAsync("New Item", "Description");

        // Assert
        result.Name.Should().Be("New Item");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public async Task UpdateItemAsync_ThrowsException_WhenItemNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Item?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateItemAsync(1, "Updated", "Description"));
    }
}
```

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ApplicationSettings": {
    "Name": "MyApp",
    "Version": "1.0.0"
  }
}
```

## Features

This template includes:

- ✅ Layered architecture (Presentation, Application, Core, Infrastructure)
- ✅ Dependency injection configured
- ✅ Command-line argument parsing with System.CommandLine
- ✅ Logging with Microsoft.Extensions.Logging
- ✅ Unit tests with xUnit, Moq, and FluentAssertions
- ✅ Repository pattern implementation
- ✅ Service layer with DTOs
- ✅ Clean separation of concerns
- ✅ SOLID principles applied
- ✅ Async/await patterns throughout

## Next Steps

1. Add Entity Framework Core for database persistence
2. Implement additional commands
3. Add configuration management
4. Implement error handling middleware
5. Add integration tests
6. Set up CI/CD pipeline

## License

[Your License Here]
