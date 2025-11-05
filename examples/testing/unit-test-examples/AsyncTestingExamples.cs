namespace UniGetUI.Examples.Testing;

/// <summary>
/// Example async service for demonstrating async testing
/// </summary>
public class DataService
{
    private readonly HttpClient _httpClient;
    
    public DataService(HttpClient httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }
    
    public async Task<string> FetchDataAsync(string url)
    {
        await Task.Delay(100); // Simulate network delay
        return $"Data from {url}";
    }
    
    public async Task<int> CalculateAsync(int value)
    {
        await Task.Delay(50);
        return value * 2;
    }
    
    public async Task<List<string>> GetItemsAsync(int count)
    {
        await Task.Delay(100);
        var items = new List<string>();
        for (int i = 0; i < count; i++)
        {
            items.Add($"Item {i + 1}");
        }
        return items;
    }
    
    public async Task ThrowExceptionAsync()
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Something went wrong");
    }
    
    public async Task<int> ProcessWithTimeoutAsync(int delay, CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken);
        return 42;
    }
}

/// <summary>
/// Examples of testing async methods properly
/// </summary>
public class AsyncTestingExamples
{
    [Fact]
    public async Task FetchDataAsync_ReturnsExpectedData()
    {
        // Arrange
        var service = new DataService();
        var url = "https://example.com/api/data";
        
        // Act
        var result = await service.FetchDataAsync(url);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains(url, result);
    }
    
    [Theory]
    [InlineData(5, 10)]
    [InlineData(10, 20)]
    [InlineData(0, 0)]
    [InlineData(-5, -10)]
    public async Task CalculateAsync_VariousInputs_ReturnsCorrectResult(int input, int expected)
    {
        // Arrange
        var service = new DataService();
        
        // Act
        var result = await service.CalculateAsync(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task GetItemsAsync_ReturnsCorrectNumberOfItems(int count)
    {
        // Arrange
        var service = new DataService();
        
        // Act
        var items = await service.GetItemsAsync(count);
        
        // Assert
        Assert.Equal(count, items.Count);
        if (count > 0)
        {
            Assert.All(items, item => Assert.StartsWith("Item ", item));
        }
    }
    
    [Fact]
    public async Task ThrowExceptionAsync_ThrowsExpectedException()
    {
        // Arrange
        var service = new DataService();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.ThrowExceptionAsync()
        );
        
        Assert.Equal("Something went wrong", exception.Message);
    }
    
    [Fact]
    public async Task ProcessWithTimeout_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var service = new DataService();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // Cancel after 50ms
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await service.ProcessWithTimeoutAsync(1000, cts.Token);
        });
    }
    
    [Fact]
    public async Task MultipleAsyncOperations_AllComplete()
    {
        // Arrange
        var service = new DataService();
        
        // Act
        var task1 = service.CalculateAsync(5);
        var task2 = service.CalculateAsync(10);
        var task3 = service.CalculateAsync(15);
        
        var results = await Task.WhenAll(task1, task2, task3);
        
        // Assert
        Assert.Equal(3, results.Length);
        Assert.Equal(10, results[0]);
        Assert.Equal(20, results[1]);
        Assert.Equal(30, results[2]);
    }
}

/// <summary>
/// Example task-based operations
/// </summary>
public class TaskProcessor
{
    public Task<int> ProcessAsync(int value)
    {
        return Task.FromResult(value * 2);
    }
    
    public async Task<int> ProcessWithDelayAsync(int value, int delayMs)
    {
        await Task.Delay(delayMs);
        return value * 2;
    }
    
    public Task<string> GetCachedValueAsync(string key)
    {
        // Simulating cached value retrieval
        return Task.FromResult($"Cached: {key}");
    }
}

/// <summary>
/// Testing Task-based patterns
/// </summary>
public class TaskBasedTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsCompletedTask()
    {
        // Arrange
        var processor = new TaskProcessor();
        
        // Act
        var result = await processor.ProcessAsync(21);
        
        // Assert
        Assert.Equal(42, result);
    }
    
    [Fact]
    public async Task ProcessWithDelayAsync_CompletesAfterDelay()
    {
        // Arrange
        var processor = new TaskProcessor();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Act
        var result = await processor.ProcessWithDelayAsync(21, 100);
        stopwatch.Stop();
        
        // Assert
        Assert.Equal(42, result);
        Assert.True(stopwatch.ElapsedMilliseconds >= 100, 
            $"Expected at least 100ms, got {stopwatch.ElapsedMilliseconds}ms");
    }
    
    [Fact]
    public async Task GetCachedValueAsync_ReturnsImmediately()
    {
        // Arrange
        var processor = new TaskProcessor();
        
        // Act
        var result = await processor.GetCachedValueAsync("testKey");
        
        // Assert
        Assert.Equal("Cached: testKey", result);
    }
}

/// <summary>
/// Example async enumerable operations
/// </summary>
public class AsyncEnumerableService
{
    public async IAsyncEnumerable<int> GenerateNumbersAsync(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Delay(10); // Simulate async work
            yield return i;
        }
    }
    
    public async IAsyncEnumerable<string> ProcessItemsAsync(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            await Task.Delay(10);
            yield return item.ToUpper();
        }
    }
}

/// <summary>
/// Testing async enumerable operations
/// </summary>
public class AsyncEnumerableTests
{
    [Fact]
    public async Task GenerateNumbersAsync_ReturnsCorrectCount()
    {
        // Arrange
        var service = new AsyncEnumerableService();
        var numbers = new List<int>();
        
        // Act
        await foreach (var number in service.GenerateNumbersAsync(5))
        {
            numbers.Add(number);
        }
        
        // Assert
        Assert.Equal(5, numbers.Count);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, numbers);
    }
    
    [Fact]
    public async Task ProcessItemsAsync_TransformsItems()
    {
        // Arrange
        var service = new AsyncEnumerableService();
        var input = new[] { "hello", "world" };
        var results = new List<string>();
        
        // Act
        await foreach (var item in service.ProcessItemsAsync(input))
        {
            results.Add(item);
        }
        
        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("HELLO", results[0]);
        Assert.Equal("WORLD", results[1]);
    }
}

/// <summary>
/// Example parallel async operations
/// </summary>
public class ParallelAsyncService
{
    public async Task<int> ProcessParallelAsync(IEnumerable<int> values)
    {
        var tasks = values.Select(async v =>
        {
            await Task.Delay(10);
            return v * 2;
        });
        
        var results = await Task.WhenAll(tasks);
        return results.Sum();
    }
    
    public async Task<Dictionary<string, string>> FetchMultipleAsync(IEnumerable<string> urls)
    {
        var results = new Dictionary<string, string>();
        var tasks = urls.Select(async url =>
        {
            await Task.Delay(10);
            return (url, $"Data from {url}");
        });
        
        var completedTasks = await Task.WhenAll(tasks);
        
        foreach (var (url, data) in completedTasks)
        {
            results[url] = data;
        }
        
        return results;
    }
}

/// <summary>
/// Testing parallel async operations
/// </summary>
public class ParallelAsyncTests
{
    [Fact]
    public async Task ProcessParallelAsync_ProcessesAllValues()
    {
        // Arrange
        var service = new ParallelAsyncService();
        var values = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        var result = await service.ProcessParallelAsync(values);
        
        // Assert
        // (1*2) + (2*2) + (3*2) + (4*2) + (5*2) = 30
        Assert.Equal(30, result);
    }
    
    [Fact]
    public async Task FetchMultipleAsync_ReturnsAllResults()
    {
        // Arrange
        var service = new ParallelAsyncService();
        var urls = new[] { "url1", "url2", "url3" };
        
        // Act
        var results = await service.FetchMultipleAsync(urls);
        
        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(urls, url => Assert.True(results.ContainsKey(url)));
        Assert.All(results.Values, value => Assert.StartsWith("Data from ", value));
    }
}
