using System.Collections.Concurrent;

namespace WpfReferenceApp.Services;

/// <summary>
/// Static class that helps reduce the CPU impact of calling CPU-intensive methods
/// that are expected to return the same result when called concurrently.
/// 
/// Demonstrates Flyweight/Task Recycler Pattern
/// 
/// Benefits:
/// - Reduces CPU usage by avoiding duplicate work
/// - Improves performance for expensive operations
/// - Provides optional caching of results
/// - Thread-safe implementation
/// 
/// Example Usage:
/// <code>
/// var task1 = TaskRecycler&lt;List&lt;Package&gt;&gt;.RunOrAttachAsync(LoadPackages);
/// var task2 = TaskRecycler&lt;List&lt;Package&gt;&gt;.RunOrAttachAsync(LoadPackages);
/// // task2 will attach to task1 if it's the same operation
/// </code>
/// </summary>
/// <typeparam name="ReturnT">The return type of the task</typeparam>
public static class TaskRecycler<ReturnT>
{
    private static readonly ConcurrentDictionary<int, Task<ReturnT>> _tasks = new();

    /// <summary>
    /// Runs the given method asynchronously or attaches to an existing task if the same method is already running
    /// </summary>
    /// <param name="method">The method to run</param>
    /// <param name="cacheTimeSecs">Optional: Time in seconds to cache the result. 0 = no caching after completion</param>
    /// <returns>A task representing the operation</returns>
    public static Task<ReturnT> RunOrAttachAsync(Func<ReturnT> method, int cacheTimeSecs = 0)
    {
        int hash = method.GetHashCode();
        return _runTaskAndWait(new Task<ReturnT>(method), hash, cacheTimeSecs);
    }

    /// <summary>
    /// Runs the given async method or attaches to an existing task if the same method is already running
    /// </summary>
    /// <param name="method">The async method to run</param>
    /// <param name="cacheTimeSecs">Optional: Time in seconds to cache the result. 0 = no caching after completion</param>
    /// <returns>A task representing the operation</returns>
    public static Task<ReturnT> RunOrAttachAsync(Func<Task<ReturnT>> method, int cacheTimeSecs = 0)
    {
        int hash = method.GetHashCode();
        return _runTaskAndWait(Task.Run(method), hash, cacheTimeSecs);
    }

    private static async Task<ReturnT> _runTaskAndWait(Task<ReturnT> task, int hash, int cacheTimeSecs)
    {
        // Try to get existing task
        if (_tasks.TryGetValue(hash, out Task<ReturnT>? existingTask))
        {
            Utilities.Logger.Info($"Attaching to existing task (hash: {hash})");
            return await existingTask.ConfigureAwait(false);
        }

        // Try to add new task
        if (_tasks.TryAdd(hash, task))
        {
            Utilities.Logger.Info($"Starting new task (hash: {hash})");
            task.Start();
        }
        else
        {
            // Race condition - another thread added a task between TryGetValue and TryAdd
            Utilities.Logger.Info($"Race condition detected, attaching to task added by another thread (hash: {hash})");
            existingTask = _tasks[hash];
            return await existingTask.ConfigureAwait(false);
        }

        // Wait for task completion
        ReturnT result = await task.ConfigureAwait(false);

        // Handle caching after completion
        if (cacheTimeSecs <= 0)
        {
            // No caching - remove immediately after completion
            _tasks.TryRemove(hash, out _);
            Utilities.Logger.Info($"Task completed, removed from cache (hash: {hash})");
        }
        else
        {
            // Cache for specified time
            Utilities.Logger.Info($"Task completed, caching result for {cacheTimeSecs} seconds (hash: {hash})");
            _ = Task.Run(async () =>
            {
                await Task.Delay(cacheTimeSecs * 1000).ConfigureAwait(false);
                _tasks.TryRemove(hash, out _);
                Utilities.Logger.Info($"Cached result expired and removed (hash: {hash})");
            });
        }

        return result;
    }

    /// <summary>
    /// Clears all cached tasks (useful for testing)
    /// </summary>
    public static void ClearCache()
    {
        _tasks.Clear();
        Utilities.Logger.Info("Task cache cleared");
    }

    /// <summary>
    /// Gets the number of currently cached tasks
    /// </summary>
    public static int CachedTaskCount => _tasks.Count;
}
