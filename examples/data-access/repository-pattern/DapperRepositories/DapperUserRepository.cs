using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using DataAccessExamples.Models;

namespace DataAccessExamples.DapperRepositories;

/// <summary>
/// User repository implementation using Dapper for high performance
/// </summary>
public class DapperUserRepository
{
    private readonly string _connectionString;

    public DapperUserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT UserId, Username, Email, IsActive, CreatedAt, UpdatedAt
            FROM Users
            WHERE UserId = @UserId";
        
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT UserId, Username, Email, IsActive, CreatedAt, UpdatedAt
            FROM Users
            ORDER BY Username";
        
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT UserId, Username, Email, IsActive, CreatedAt, UpdatedAt
            FROM Users
            WHERE IsActive = 1
            ORDER BY Username";
        
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT UserId, Username, Email, IsActive, CreatedAt, UpdatedAt
            FROM Users
            WHERE Username = @Username";
        
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            INSERT INTO Users (Username, Email, IsActive, CreatedAt)
            VALUES (@Username, @Email, @IsActive, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<int> UpdateAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            UPDATE Users
            SET Username = @Username,
                Email = @Email,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE UserId = @UserId";
        
        return await connection.ExecuteAsync(sql, user);
    }

    public async Task<int> DeleteAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = "DELETE FROM Users WHERE UserId = @UserId";
        
        return await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<User>> GetUsersWithOrdersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT u.UserId, u.Username, u.Email, u.IsActive, u.CreatedAt, u.UpdatedAt,
                   o.OrderId, o.UserId, o.OrderDate, o.TotalAmount, o.Status
            FROM Users u
            LEFT JOIN Orders o ON u.UserId = o.UserId
            ORDER BY u.Username, o.OrderDate DESC";
        
        var userDict = new Dictionary<int, User>();
        
        await connection.QueryAsync<User, Order, User>(
            sql,
            (user, order) =>
            {
                if (!userDict.TryGetValue(user.UserId, out var currentUser))
                {
                    currentUser = user;
                    currentUser.Orders = new List<Order>();
                    userDict.Add(user.UserId, currentUser);
                }
                
                if (order != null)
                {
                    currentUser.Orders.Add(order);
                }
                
                return currentUser;
            },
            splitOn: "OrderId");
        
        return userDict.Values;
    }
}
