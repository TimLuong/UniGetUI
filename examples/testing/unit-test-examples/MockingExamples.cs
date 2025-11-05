using Moq;

namespace UniGetUI.Examples.Testing;

/// <summary>
/// Example interfaces and classes for demonstrating mocking
/// </summary>
public interface IPackageRepository
{
    Task<Package> GetPackageByIdAsync(string id);
    Task<List<Package>> GetAllPackagesAsync();
    Task<bool> SavePackageAsync(Package package);
    Task<bool> DeletePackageAsync(string id);
}

public class Package
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Source { get; set; }
}

public interface ILogger
{
    void Log(string message);
    void LogError(string message, Exception exception);
    void LogInfo(string message);
}

public class PackageService
{
    private readonly IPackageRepository _repository;
    private readonly ILogger _logger;
    
    public PackageService(IPackageRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<Package> GetPackageAsync(string id)
    {
        try
        {
            _logger.LogInfo($"Fetching package: {id}");
            var package = await _repository.GetPackageByIdAsync(id);
            
            if (package == null)
            {
                _logger.LogInfo($"Package not found: {id}");
            }
            
            return package;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching package: {id}", ex);
            throw;
        }
    }
    
    public async Task<bool> InstallPackageAsync(string id)
    {
        var package = await _repository.GetPackageByIdAsync(id);
        
        if (package == null)
        {
            _logger.LogError($"Cannot install non-existent package: {id}", null);
            return false;
        }
        
        _logger.LogInfo($"Installing package: {package.Name}");
        return true;
    }
    
    public async Task<int> GetPackageCountAsync()
    {
        var packages = await _repository.GetAllPackagesAsync();
        return packages?.Count ?? 0;
    }
}

/// <summary>
/// Examples demonstrating mocking with Moq
/// </summary>
public class MockingExamples
{
    [Fact]
    public async Task GetPackageAsync_WithValidId_ReturnsPackage()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockLogger = new Mock<ILogger>();
        
        var expectedPackage = new Package
        {
            Id = "test.package",
            Name = "Test Package",
            Version = "1.0.0"
        };
        
        mockRepository
            .Setup(r => r.GetPackageByIdAsync("test.package"))
            .ReturnsAsync(expectedPackage);
        
        var service = new PackageService(mockRepository.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetPackageAsync("test.package");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.package", result.Id);
        Assert.Equal("Test Package", result.Name);
        
        // Verify logger was called
        mockLogger.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Fetching package"))), Times.Once);
    }
    
    [Fact]
    public async Task GetPackageAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockLogger = new Mock<ILogger>();
        
        mockRepository
            .Setup(r => r.GetPackageByIdAsync("invalid.id"))
            .ReturnsAsync((Package)null);
        
        var service = new PackageService(mockRepository.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetPackageAsync("invalid.id");
        
        // Assert
        Assert.Null(result);
        mockLogger.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("not found"))), Times.Once);
    }
    
    [Fact]
    public async Task GetPackageAsync_WhenRepositoryThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockLogger = new Mock<ILogger>();
        
        var expectedException = new InvalidOperationException("Database error");
        
        mockRepository
            .Setup(r => r.GetPackageByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(expectedException);
        
        var service = new PackageService(mockRepository.Object, mockLogger.Object);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.GetPackageAsync("any.id")
        );
        
        mockLogger.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("Error fetching package")),
            It.IsAny<Exception>()
        ), Times.Once);
    }
    
    [Fact]
    public async Task InstallPackageAsync_WithExistingPackage_ReturnsTrue()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockLogger = new Mock<ILogger>();
        
        var package = new Package
        {
            Id = "test.package",
            Name = "Test Package"
        };
        
        mockRepository
            .Setup(r => r.GetPackageByIdAsync("test.package"))
            .ReturnsAsync(package);
        
        var service = new PackageService(mockRepository.Object, mockLogger.Object);
        
        // Act
        var result = await service.InstallPackageAsync("test.package");
        
        // Assert
        Assert.True(result);
        mockLogger.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Installing"))), Times.Once);
    }
    
    [Fact]
    public async Task InstallPackageAsync_WithNonExistentPackage_ReturnsFalse()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockLogger = new Mock<ILogger>();
        
        mockRepository
            .Setup(r => r.GetPackageByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Package)null);
        
        var service = new PackageService(mockRepository.Object, mockLogger.Object);
        
        // Act
        var result = await service.InstallPackageAsync("nonexistent.id");
        
        // Assert
        Assert.False(result);
        mockLogger.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("Cannot install")),
            null
        ), Times.Once);
    }
    
    [Fact]
    public async Task GetPackageCountAsync_WithPackages_ReturnsCorrectCount()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockLogger = new Mock<ILogger>();
        
        var packages = new List<Package>
        {
            new Package { Id = "pkg1", Name = "Package 1" },
            new Package { Id = "pkg2", Name = "Package 2" },
            new Package { Id = "pkg3", Name = "Package 3" }
        };
        
        mockRepository
            .Setup(r => r.GetAllPackagesAsync())
            .ReturnsAsync(packages);
        
        var service = new PackageService(mockRepository.Object, mockLogger.Object);
        
        // Act
        var count = await service.GetPackageCountAsync();
        
        // Assert
        Assert.Equal(3, count);
    }
}

/// <summary>
/// Advanced mocking examples
/// </summary>
public class AdvancedMockingExamples
{
    [Fact]
    public async Task MockWithCallback_CapturesArguments()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        var mockLogger = new Mock<ILogger>();
        
        Package savedPackage = null;
        
        mockRepository
            .Setup(r => r.SavePackageAsync(It.IsAny<Package>()))
            .Callback<Package>(pkg => savedPackage = pkg)
            .ReturnsAsync(true);
        
        var service = new PackageService(mockRepository.Object, mockLogger.Object);
        
        // Act
        var packageToSave = new Package { Id = "test", Name = "Test" };
        await mockRepository.Object.SavePackageAsync(packageToSave);
        
        // Assert
        Assert.NotNull(savedPackage);
        Assert.Equal("test", savedPackage.Id);
    }
    
    [Fact]
    public async Task MockWithSequence_ReturnsDifferentValuesOnSuccessiveCalls()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        
        mockRepository
            .SetupSequence(r => r.GetPackageByIdAsync("test"))
            .ReturnsAsync(new Package { Id = "test", Name = "First" })
            .ReturnsAsync(new Package { Id = "test", Name = "Second" })
            .ReturnsAsync(new Package { Id = "test", Name = "Third" });
        
        // Act & Assert
        var first = await mockRepository.Object.GetPackageByIdAsync("test");
        Assert.Equal("First", first.Name);
        
        var second = await mockRepository.Object.GetPackageByIdAsync("test");
        Assert.Equal("Second", second.Name);
        
        var third = await mockRepository.Object.GetPackageByIdAsync("test");
        Assert.Equal("Third", third.Name);
    }
    
    [Fact]
    public async Task MockWithPredicate_MatchesSpecificArguments()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>();
        
        mockRepository
            .Setup(r => r.GetPackageByIdAsync(It.Is<string>(id => id.StartsWith("test."))))
            .ReturnsAsync(new Package { Id = "matched", Name = "Matched Package" });
        
        mockRepository
            .Setup(r => r.GetPackageByIdAsync(It.Is<string>(id => !id.StartsWith("test."))))
            .ReturnsAsync((Package)null);
        
        // Act
        var matchedResult = await mockRepository.Object.GetPackageByIdAsync("test.package");
        var unmatchedResult = await mockRepository.Object.GetPackageByIdAsync("other.package");
        
        // Assert
        Assert.NotNull(matchedResult);
        Assert.Equal("Matched Package", matchedResult.Name);
        Assert.Null(unmatchedResult);
    }
    
    [Fact]
    public void VerifyMethodCalls_WithSpecificParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        
        // Act
        mockLogger.Object.Log("Message 1");
        mockLogger.Object.Log("Message 2");
        mockLogger.Object.LogInfo("Info message");
        
        // Assert - Verify specific calls
        mockLogger.Verify(l => l.Log("Message 1"), Times.Once);
        mockLogger.Verify(l => l.Log(It.IsAny<string>()), Times.Exactly(2));
        mockLogger.Verify(l => l.LogInfo(It.IsAny<string>()), Times.Once);
        mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }
    
    [Fact]
    public void VerifyNoOtherCalls_EnsuresOnlyExpectedCallsAreMade()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        
        // Act
        mockLogger.Object.Log("Test message");
        
        // Assert
        mockLogger.Verify(l => l.Log("Test message"), Times.Once);
        mockLogger.VerifyNoOtherCalls();
    }
}

/// <summary>
/// Examples of partial mocking and strict behavior
/// </summary>
public class StrictMockingExamples
{
    [Fact]
    public void StrictMock_ThrowsWhenUnexpectedMethodCalled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
        
        // Setup only expected method
        mockLogger.Setup(l => l.Log("Expected message"));
        
        // Act - This works
        mockLogger.Object.Log("Expected message");
        
        // Act & Assert - This throws because it's not setup
        Assert.Throws<MockException>(() => mockLogger.Object.Log("Unexpected message"));
    }
    
    [Fact]
    public async Task LooseMock_ReturnsDefaultWhenNotSetup()
    {
        // Arrange
        var mockRepository = new Mock<IPackageRepository>(MockBehavior.Loose);
        // Note: No setup for GetPackageByIdAsync
        
        // Act
        var result = await mockRepository.Object.GetPackageByIdAsync("any.id");
        
        // Assert - Returns default value (null for reference types)
        Assert.Null(result);
    }
}

/// <summary>
/// Example using property mocking
/// </summary>
public interface IConfiguration
{
    string ApiUrl { get; set; }
    int Timeout { get; set; }
    bool EnableLogging { get; set; }
}

public class PropertyMockingExamples
{
    [Fact]
    public void MockProperties_CanBeGetAndSet()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.SetupProperty(c => c.ApiUrl, "https://default.url");
        mockConfig.SetupProperty(c => c.Timeout);
        
        // Act
        mockConfig.Object.ApiUrl = "https://new.url";
        mockConfig.Object.Timeout = 5000;
        
        // Assert
        Assert.Equal("https://new.url", mockConfig.Object.ApiUrl);
        Assert.Equal(5000, mockConfig.Object.Timeout);
    }
    
    [Fact]
    public void MockAllProperties_TrackAllPropertyChanges()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.SetupAllProperties();
        
        // Act
        mockConfig.Object.ApiUrl = "https://test.url";
        mockConfig.Object.Timeout = 3000;
        mockConfig.Object.EnableLogging = true;
        
        // Assert
        Assert.Equal("https://test.url", mockConfig.Object.ApiUrl);
        Assert.Equal(3000, mockConfig.Object.Timeout);
        Assert.True(mockConfig.Object.EnableLogging);
    }
}
