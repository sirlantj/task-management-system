using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;

namespace TaskManagementSystem.Application.UseCases.Tasks;

public class GetTasksUseCase
{
    private readonly ITaskRepository _taskRepository;

    public GetTasksUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskResponse>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllByUserIdAsync(userId, cancellationToken);
        return tasks.Select(CreateTaskUseCase.MapToResponse);
    }
}
