using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Application.UseCases.Tasks;

public class CreateTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> ExecuteAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            TaskStatus.Pending,
            request.DueDate,
            userId,
            DateTime.UtcNow,
            null);

        await _taskRepository.CreateAsync(task, cancellationToken);
        return MapToResponse(task);
    }

    internal static TaskResponse MapToResponse(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status.ToString(),
            task.DueDate, task.UserId, task.CreatedAt, task.UpdatedAt);
}
