using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Application.UseCases.Tasks;

public class DeleteTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task ExecuteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, userId, cancellationToken);

        if (task is null)
            throw new NotFoundException($"Task '{taskId}' not found.");

        if (!task.CanBeDeleted())
            throw new DomainException("Completed tasks cannot be deleted.");

        await _taskRepository.DeleteAsync(taskId, userId, cancellationToken);
    }
}
