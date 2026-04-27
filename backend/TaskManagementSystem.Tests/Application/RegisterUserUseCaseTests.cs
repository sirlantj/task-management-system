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

    [Theory]
    [InlineData("", "email@example.com", "password123", "Name is required")]
    [InlineData("  ", "email@example.com", "password123", "Name is required")]
    [InlineData("Alice", "", "password123", "valid email")]
    [InlineData("Alice", "not-an-email", "password123", "valid email")]
    [InlineData("Alice", "email@example.com", "short", "8 characters")]
    public async System.Threading.Tasks.Task ExecuteAsync_InvalidInput_ThrowsValidationException(
        string name, string email, string password, string expectedFragment)
    {
        var request = new RegisterUserRequest(name, email, password);
        var ex = await Assert.ThrowsAsync<ValidationException>(() => CreateUseCase().ExecuteAsync(request));
        Assert.Contains(expectedFragment, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

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
        _hasher.Setup(h => h.HashPassword("password1")).Returns("hash");
        _tokenService.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var request = new RegisterUserRequest("Bob", "bob@example.com", "password1");
        var result = await CreateUseCase().ExecuteAsync(request);

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("bob@example.com", result.Email);
        _userRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), default), Times.Once);
    }
}
