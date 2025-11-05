namespace UniGetUI.Examples.Testing.Integration;

/// <summary>
/// Example file operations service for testing
/// </summary>
public class FileService
{
    public async Task<string> ReadFileAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");
        
        return await File.ReadAllTextAsync(path);
    }
    
    public async Task WriteFileAsync(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllTextAsync(path, content);
    }
    
    public async Task<bool> CopyFileAsync(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
            return false;
        
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await Task.Run(() => File.Copy(sourcePath, destinationPath, overwrite: true));
        return true;
    }
    
    public bool DeleteFile(string path)
    {
        if (!File.Exists(path))
            return false;
        
        File.Delete(path);
        return true;
    }
    
    public List<string> GetFiles(string directory, string searchPattern = "*.*")
    {
        if (!Directory.Exists(directory))
            return new List<string>();
        
        return Directory.GetFiles(directory, searchPattern).ToList();
    }
}

/// <summary>
/// File system integration tests
/// </summary>
[Trait("Category", "Integration")]
[Trait("Layer", "FileSystem")]
public class FileSystemIntegrationTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly FileService _fileService;

    public FileSystemIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"UniGetUITests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _fileService = new FileService();
    }

    [Fact]
    public async Task WriteFile_CreatesFileWithContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        var content = "Hello, World!";
        
        // Act
        await _fileService.WriteFileAsync(filePath, content);
        
        // Assert
        Assert.True(File.Exists(filePath));
        var actualContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal(content, actualContent);
    }

    [Fact]
    public async Task WriteFile_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var subdirectory = Path.Combine(_testDirectory, "subdir", "nested");
        var filePath = Path.Combine(subdirectory, "test.txt");
        
        // Act
        await _fileService.WriteFileAsync(filePath, "Test content");
        
        // Assert
        Assert.True(Directory.Exists(subdirectory));
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task ReadFile_ReturnsFileContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "read-test.txt");
        var expectedContent = "Test content to read";
        await File.WriteAllTextAsync(filePath, expectedContent);
        
        // Act
        var actualContent = await _fileService.ReadFileAsync(filePath);
        
        // Assert
        Assert.Equal(expectedContent, actualContent);
    }

    [Fact]
    public async Task ReadFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await _fileService.ReadFileAsync(filePath)
        );
    }

    [Fact]
    public async Task CopyFile_CreatesExactCopy()
    {
        // Arrange
        var sourcePath = Path.Combine(_testDirectory, "source.txt");
        var destPath = Path.Combine(_testDirectory, "destination.txt");
        var content = "Content to copy";
        await File.WriteAllTextAsync(sourcePath, content);
        
        // Act
        var result = await _fileService.CopyFileAsync(sourcePath, destPath);
        
        // Assert
        Assert.True(result);
        Assert.True(File.Exists(destPath));
        var copiedContent = await File.ReadAllTextAsync(destPath);
        Assert.Equal(content, copiedContent);
    }

    [Fact]
    public async Task CopyFile_WithNonExistentSource_ReturnsFalse()
    {
        // Arrange
        var sourcePath = Path.Combine(_testDirectory, "nonexistent.txt");
        var destPath = Path.Combine(_testDirectory, "destination.txt");
        
        // Act
        var result = await _fileService.CopyFileAsync(sourcePath, destPath);
        
        // Assert
        Assert.False(result);
        Assert.False(File.Exists(destPath));
    }

    [Fact]
    public void DeleteFile_RemovesExistingFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "delete-me.txt");
        File.WriteAllText(filePath, "To be deleted");
        
        // Act
        var result = _fileService.DeleteFile(filePath);
        
        // Assert
        Assert.True(result);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void DeleteFile_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");
        
        // Act
        var result = _fileService.DeleteFile(filePath);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFiles_ReturnsAllFilesInDirectory()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "file1.txt");
        var file2 = Path.Combine(_testDirectory, "file2.txt");
        var file3 = Path.Combine(_testDirectory, "file3.log");
        
        File.WriteAllText(file1, "content");
        File.WriteAllText(file2, "content");
        File.WriteAllText(file3, "content");
        
        // Act
        var files = _fileService.GetFiles(_testDirectory);
        
        // Assert
        Assert.Equal(3, files.Count);
        Assert.Contains(file1, files);
        Assert.Contains(file2, files);
        Assert.Contains(file3, files);
    }

    [Fact]
    public void GetFiles_WithPattern_ReturnsMatchingFiles()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "file1.txt");
        var file2 = Path.Combine(_testDirectory, "file2.txt");
        var file3 = Path.Combine(_testDirectory, "file3.log");
        
        File.WriteAllText(file1, "content");
        File.WriteAllText(file2, "content");
        File.WriteAllText(file3, "content");
        
        // Act
        var txtFiles = _fileService.GetFiles(_testDirectory, "*.txt");
        
        // Assert
        Assert.Equal(2, txtFiles.Count);
        Assert.Contains(file1, txtFiles);
        Assert.Contains(file2, txtFiles);
        Assert.DoesNotContain(file3, txtFiles);
    }

    [Fact]
    public void GetFiles_WithNonExistentDirectory_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");
        
        // Act
        var files = _fileService.GetFiles(nonExistentDir);
        
        // Assert
        Assert.Empty(files);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

/// <summary>
/// Example package export service for testing
/// </summary>
public class PackageExporter
{
    private readonly FileService _fileService;
    
    public PackageExporter(FileService fileService = null)
    {
        _fileService = fileService ?? new FileService();
    }
    
    public async Task ExportPackagesToJsonAsync(List<Package> packages, string outputPath)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(packages, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        await _fileService.WriteFileAsync(outputPath, json);
    }
    
    public async Task<List<Package>> ImportPackagesFromJsonAsync(string inputPath)
    {
        var json = await _fileService.ReadFileAsync(inputPath);
        return System.Text.Json.JsonSerializer.Deserialize<List<Package>>(json);
    }
}

public class Package
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Source { get; set; }
}

/// <summary>
/// Integration tests for package export/import
/// </summary>
[Trait("Category", "Integration")]
[Trait("Layer", "FileSystem")]
public class PackageExportIntegrationTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly PackageExporter _exporter;

    public PackageExportIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PackageExportTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _exporter = new PackageExporter();
    }

    [Fact]
    public async Task ExportPackages_CreatesValidJsonFile()
    {
        // Arrange
        var packages = new List<Package>
        {
            new Package { Id = "pkg1", Name = "Package 1", Version = "1.0.0", Source = "WinGet" },
            new Package { Id = "pkg2", Name = "Package 2", Version = "2.0.0", Source = "Chocolatey" }
        };
        var outputPath = Path.Combine(_testDirectory, "packages.json");
        
        // Act
        await _exporter.ExportPackagesToJsonAsync(packages, outputPath);
        
        // Assert
        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("pkg1", content);
        Assert.Contains("Package 1", content);
        Assert.Contains("WinGet", content);
    }

    [Fact]
    public async Task ImportPackages_ReadsValidJsonFile()
    {
        // Arrange
        var originalPackages = new List<Package>
        {
            new Package { Id = "test1", Name = "Test Package 1", Version = "1.0.0", Source = "WinGet" },
            new Package { Id = "test2", Name = "Test Package 2", Version = "2.0.0", Source = "Scoop" }
        };
        var filePath = Path.Combine(_testDirectory, "import.json");
        await _exporter.ExportPackagesToJsonAsync(originalPackages, filePath);
        
        // Act
        var importedPackages = await _exporter.ImportPackagesFromJsonAsync(filePath);
        
        // Assert
        Assert.NotNull(importedPackages);
        Assert.Equal(2, importedPackages.Count);
        Assert.Equal("test1", importedPackages[0].Id);
        Assert.Equal("Test Package 1", importedPackages[0].Name);
        Assert.Equal("test2", importedPackages[1].Id);
        Assert.Equal("Test Package 2", importedPackages[1].Name);
    }

    [Fact]
    public async Task ExportImportRoundTrip_PreservesData()
    {
        // Arrange
        var originalPackages = new List<Package>
        {
            new Package { Id = "round1", Name = "Round Trip 1", Version = "1.0.0", Source = "WinGet" },
            new Package { Id = "round2", Name = "Round Trip 2", Version = "2.5.0", Source = "Chocolatey" },
            new Package { Id = "round3", Name = "Round Trip 3", Version = "3.0.1", Source = "Scoop" }
        };
        var filePath = Path.Combine(_testDirectory, "roundtrip.json");
        
        // Act
        await _exporter.ExportPackagesToJsonAsync(originalPackages, filePath);
        var importedPackages = await _exporter.ImportPackagesFromJsonAsync(filePath);
        
        // Assert
        Assert.Equal(originalPackages.Count, importedPackages.Count);
        for (int i = 0; i < originalPackages.Count; i++)
        {
            Assert.Equal(originalPackages[i].Id, importedPackages[i].Id);
            Assert.Equal(originalPackages[i].Name, importedPackages[i].Name);
            Assert.Equal(originalPackages[i].Version, importedPackages[i].Version);
            Assert.Equal(originalPackages[i].Source, importedPackages[i].Source);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
