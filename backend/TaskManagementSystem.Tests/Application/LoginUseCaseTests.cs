using Moq;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.UseCases.Auth;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Tests.Application;

public class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokenService = new();

    private LoginUseCase CreateUseCase() =>
        new(_userRepo.Object, _hasher.Object, _tokenService.Object);

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        _userRepo.Setup(r => r.FindByEmailAsync("ghost@example.com", default)).ReturnsAsync((User?)null);

        var request = new LoginRequest("ghost@example.com", "password");

        await Assert.ThrowsAsync<UnauthorizedException>(() => CreateUseCase().ExecuteAsync(request));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = new User(Guid.NewGuid(), "Alice", "alice@example.com", "hash", "salt", DateTime.UtcNow);
        _userRepo.Setup(r => r.FindByEmailAsync("alice@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.VerifyPassword("wrong", "hash", "salt")).Returns(false);

        var request = new LoginRequest("alice@example.com", "wrong");

        await Assert.ThrowsAsync<UnauthorizedException>(() => CreateUseCase().ExecuteAsync(request));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_ValidCredentials_ReturnsToken()
    {
        var user = new User(Guid.NewGuid(), "Alice", "alice@example.com", "hash", "salt", DateTime.UtcNow);
        _userRepo.Setup(r => r.FindByEmailAsync("alice@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.VerifyPassword("correct", "hash", "salt")).Returns(true);
        _tokenService.Setup(t => t.GenerateToken(user)).Returns("jwt-token");

        var result = await CreateUseCase().ExecuteAsync(new LoginRequest("alice@example.com", "correct"));

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal(user.Id, result.UserId);
    }
}
