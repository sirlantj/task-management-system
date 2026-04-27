using Moq;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.UseCases.Tasks;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Tests.Application;

public class GetTasksUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();

    private GetTasksUseCase CreateUseCase() => new(_taskRepo.Object);

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_ReturnsOnlyTasksForAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new(Guid.NewGuid(), "Task A", null, TaskStatus.Pending, null, userId, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "Task B", null, TaskStatus.InProgress, null, userId, DateTime.UtcNow, null),
        };

        _taskRepo.Setup(r => r.GetAllByUserIdAsync(userId, default)).ReturnsAsync(tasks);

        var result = (await CreateUseCase().ExecuteAsync(userId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(userId, r.UserId));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_NoTasks_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _taskRepo.Setup(r => r.GetAllByUserIdAsync(userId, default)).ReturnsAsync(new List<TaskItem>());

        var result = await CreateUseCase().ExecuteAsync(userId);

        Assert.Empty(result);
    }
}
