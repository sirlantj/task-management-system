using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
