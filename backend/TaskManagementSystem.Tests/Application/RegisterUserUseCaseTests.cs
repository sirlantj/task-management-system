using Moq;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.UseCases.Auth;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Tests.Application;

public class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokenService = new();

    private RegisterUserUseCase CreateUseCase() =>
        new(_userRepo.Object, _hasher.Object, _tokenService.Object);

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DuplicateEmail_ThrowsConflictException()
    {
        var existing = new User(Guid.NewGuid(), "Alice", "alice@example.com", "hash", "salt", DateTime.UtcNow);
        _userRepo.Setup(r => r.FindByEmailAsync("alice@example.com", default)).ReturnsAsync(existing);

        var request = new RegisterUserRequest("Alice", "alice@example.com", "password");

        await Assert.ThrowsAsync<ConflictException>(() => CreateUseCase().ExecuteAsync(request));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_NewEmail_CreatesUserAndReturnsToken()
    {
        _userRepo.Setup(r => r.FindByEmailAsync("bob@example.com", default)).ReturnsAsync((User?)null);
        _hasher.Setup(h => h.HashPassword("password")).Returns(("hash", "salt"));
        _tokenService.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var request = new RegisterUserRequest("Bob", "bob@example.com", "password");
        var result = await CreateUseCase().ExecuteAsync(request);

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("bob@example.com", result.Email);
        _userRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), default), Times.Once);
    }
}
