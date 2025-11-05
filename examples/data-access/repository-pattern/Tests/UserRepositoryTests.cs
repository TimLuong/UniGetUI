using Microsoft.EntityFrameworkCore;
using Xunit;
using DataAccessExamples.Data;
using DataAccessExamples.Models;
using DataAccessExamples.Repositories;

namespace DataAccessExamples.Tests;

/// <summary>
/// Unit tests for UserRepository using in-memory database
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true
        };
        var addedUser = await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(addedUser.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsUser_WhenUsernameExists()
    {
        // Arrange
        var user = new User
        {
            Username = "johndoe",
            Email = "john@example.com"
        };
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByUsernameAsync("johndoe");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
    }

    [Fact]
    public async Task GetActiveUsersAsync_ReturnsOnlyActiveUsers()
    {
        // Arrange
        await _repository.AddAsync(new User { Username = "active1", Email = "active1@test.com", IsActive = true });
        await _repository.AddAsync(new User { Username = "active2", Email = "active2@test.com", IsActive = true });
        await _repository.AddAsync(new User { Username = "inactive", Email = "inactive@test.com", IsActive = false });

        // Act
        var result = await _repository.GetActiveUsersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, user => Assert.True(user.IsActive));
    }

    [Fact]
    public async Task AddAsync_AddsUserToDatabase()
    {
        // Arrange
        var user = new User
        {
            Username = "newuser",
            Email = "new@example.com"
        };

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        Assert.NotEqual(0, result.UserId);
        var savedUser = await _repository.GetByIdAsync(result.UserId);
        Assert.NotNull(savedUser);
        Assert.Equal("newuser", savedUser.Username);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUserInDatabase()
    {
        // Arrange
        var user = new User
        {
            Username = "oldname",
            Email = "old@example.com"
        };
        var addedUser = await _repository.AddAsync(user);

        // Act
        addedUser.Email = "new@example.com";
        addedUser.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(addedUser);

        // Assert
        var updatedUser = await _repository.GetByIdAsync(addedUser.UserId);
        Assert.NotNull(updatedUser);
        Assert.Equal("new@example.com", updatedUser.Email);
        Assert.NotNull(updatedUser.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesUserFromDatabase()
    {
        // Arrange
        var user = new User
        {
            Username = "deleteme",
            Email = "delete@example.com"
        };
        var addedUser = await _repository.AddAsync(user);

        // Act
        await _repository.DeleteAsync(addedUser.UserId);

        // Assert
        var deletedUser = await _repository.GetByIdAsync(addedUser.UserId);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsMatchingUsers()
    {
        // Arrange
        await _repository.AddAsync(new User { Username = "john_smith", Email = "john@test.com" });
        await _repository.AddAsync(new User { Username = "jane_doe", Email = "jane@test.com" });
        await _repository.AddAsync(new User { Username = "bob_johnson", Email = "bob@example.com" });

        // Act
        var result = await _repository.SearchUsersAsync("john");

        // Assert
        Assert.Equal(2, result.Count); // john_smith and bob_johnson
        Assert.All(result, user => 
            Assert.True(user.Username.Contains("john", StringComparison.OrdinalIgnoreCase) || 
                       user.Email.Contains("john", StringComparison.OrdinalIgnoreCase)));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
