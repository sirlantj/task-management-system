using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;

namespace TaskManagementSystem.Application.UseCases.Auth;

public class GetCurrentUserUseCase
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<MeResponse> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User '{userId}' not found.");

        return new MeResponse(user.Id, user.Name, user.Email);
    }
}
