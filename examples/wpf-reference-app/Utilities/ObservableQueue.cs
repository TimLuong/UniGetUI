namespace WpfReferenceApp.Utilities;

/// <summary>
/// Demonstrates Observer Pattern with custom event args
/// An observable queue that raises events when items are enqueued or dequeued
/// </summary>
/// <typeparam name="T">Type of items in the queue</typeparam>
public class ObservableQueue<T> : Queue<T>
{
    /// <summary>
    /// Custom EventArgs using C# 12 primary constructor
    /// </summary>
    public class EventArgs(T item) : System.EventArgs
    {
        public readonly T Item = item;
    }

    /// <summary>
    /// Event raised when an item is enqueued
    /// </summary>
    public event EventHandler<EventArgs>? ItemEnqueued;

    /// <summary>
    /// Event raised when an item is dequeued
    /// </summary>
    public event EventHandler<EventArgs>? ItemDequeued;

    /// <summary>
    /// Enqueues an item and raises the ItemEnqueued event
    /// </summary>
    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        ItemEnqueued?.Invoke(this, new EventArgs(item));
    }

    /// <summary>
    /// Dequeues an item and raises the ItemDequeued event
    /// </summary>
    public new T Dequeue()
    {
        T item = base.Dequeue();
        ItemDequeued?.Invoke(this, new EventArgs(item));
        return item;
    }

    /// <summary>
    /// Tries to dequeue an item and raises the ItemDequeued event if successful
    /// </summary>
    public bool TryDequeue(out T? result)
    {
        if (Count > 0)
        {
            result = Dequeue();
            return true;
        }
        result = default;
        return false;
    }
}
