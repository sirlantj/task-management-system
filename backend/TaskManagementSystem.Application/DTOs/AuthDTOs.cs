namespace TaskManagementSystem.Application.DTOs;

public record RegisterUserRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, Guid UserId, string Name, string Email);

public record MeResponse(Guid Id, string Name, string Email);
