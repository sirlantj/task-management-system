using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Application.UseCases.Tasks;

public class UpdateTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public UpdateTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> ExecuteAsync(Guid taskId, Guid userId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, userId, cancellationToken);

        if (task is null)
            throw new NotFoundException($"Task '{taskId}' not found.");

        task.UpdateTitle(request.Title);
        task.UpdateDescription(request.Description);
        task.UpdateDueDate(request.DueDate);

        if (request.Status is not null)
        {
            if (!Enum.TryParse<TaskStatus>(request.Status, ignoreCase: true, out var newStatus))
                throw new DomainException($"'{request.Status}' is not a valid task status.");

            task.TransitionStatus(newStatus);
        }

        await _taskRepository.UpdateAsync(task, cancellationToken);
        return TaskMapper.ToResponse(task);
    }
}
