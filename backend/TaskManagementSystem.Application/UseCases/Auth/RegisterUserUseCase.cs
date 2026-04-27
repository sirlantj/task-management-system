using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Application.UseCases.Auth;

public class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var hash = _passwordHasher.HashPassword(request.Password);
        var user = new User(Guid.NewGuid(), request.Name, request.Email, hash, string.Empty, DateTime.UtcNow);

        await _userRepository.CreateAsync(user, cancellationToken);

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Name, user.Email);
    }
}
