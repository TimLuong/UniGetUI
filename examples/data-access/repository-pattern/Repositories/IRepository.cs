namespace DataAccessExamples.Repositories;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if entity exists by ID
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
