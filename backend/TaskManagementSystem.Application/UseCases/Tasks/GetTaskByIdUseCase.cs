using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;

namespace TaskManagementSystem.Application.UseCases.Tasks;

public class GetTaskByIdUseCase
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskByIdUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> ExecuteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, userId, cancellationToken);

        if (task is null)
            throw new NotFoundException($"Task '{taskId}' not found.");

        return TaskMapper.ToResponse(task);
    }
}
