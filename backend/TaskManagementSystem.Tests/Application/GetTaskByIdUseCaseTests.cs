using Moq;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.UseCases.Tasks;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Tests.Application;

public class GetTaskByIdUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();

    private GetTaskByIdUseCase CreateUseCase() => new(_taskRepo.Object);

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_OwnTask_ReturnsTaskResponse()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem(taskId, "My task", null, TaskStatus.Pending, null, userId, DateTime.UtcNow, null);

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync(task);

        var result = await CreateUseCase().ExecuteAsync(taskId, userId);

        Assert.Equal(taskId, result.Id);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_AnotherUsersTask_ThrowsNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, requestingUserId, default)).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateUseCase().ExecuteAsync(taskId, requestingUserId));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_NonExistentTask_ThrowsNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateUseCase().ExecuteAsync(taskId, userId));
    }
}
