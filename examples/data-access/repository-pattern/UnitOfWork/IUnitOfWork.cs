using DataAccessExamples.Repositories;

namespace DataAccessExamples.UnitOfWork;

/// <summary>
/// Unit of Work interface for managing transactions and coordinating repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// User repository
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Order repository
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}
