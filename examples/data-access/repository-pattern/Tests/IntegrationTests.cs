using Moq;
using Xunit;
using DataAccessExamples.Models;
using DataAccessExamples.Repositories;
using DataAccessExamples.Services;

namespace DataAccessExamples.Tests;

/// <summary>
/// Integration tests demonstrating service layer testing with mocked repositories
/// </summary>
public class IntegrationTests
{
    [Fact]
    public async Task UserService_CreateUser_CallsRepository()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        mockRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.UserId = 1; return u; });

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.CreateUserAsync("testuser", "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("testuser", result.Username);
        mockRepo.Verify(r => r.GetByUsernameAsync("testuser"), Times.Once);
        mockRepo.Verify(r => r.GetByEmailAsync("test@example.com"), Times.Once);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UserService_CreateUser_ThrowsException_WhenUsernameExists()
    {
        // Arrange
        var existingUser = new User { UserId = 1, Username = "existing", Email = "existing@example.com" };
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByUsernameAsync("existing"))
            .ReturnsAsync(existingUser);

        var service = new UserService(mockRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CreateUserAsync("existing", "new@example.com"));
    }

    [Fact]
    public async Task UserService_UpdateUser_UpdatesProperties()
    {
        // Arrange
        var user = new User { UserId = 1, Username = "testuser", Email = "old@example.com", IsActive = true };
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.UpdateUserAsync(1, email: "new@example.com", isActive: false);

        // Assert
        Assert.Equal("new@example.com", result.Email);
        Assert.False(result.IsActive);
        Assert.NotNull(result.UpdatedAt);
        mockRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UserService_SearchUsers_ReturnsEmptyList_ForEmptySearchTerm()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.SearchUsersAsync("");

        // Assert
        Assert.Empty(result);
        mockRepo.Verify(r => r.SearchUsersAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UserService_DeactivateUser_SetsIsActiveToFalse()
    {
        // Arrange
        var user = new User { UserId = 1, Username = "testuser", Email = "test@example.com", IsActive = true };
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = new UserService(mockRepo.Object);

        // Act
        await service.DeactivateUserAsync(1);

        // Assert
        mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.UserId == 1 && u.IsActive == false)), Times.Once);
    }

    [Fact]
    public async Task UserService_GetUser_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.GetUserAsync(999);

        // Assert
        Assert.Null(result);
    }
}
