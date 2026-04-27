using Moq;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.UseCases.Tasks;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Tests.Application;

public class DeleteTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();

    private DeleteTaskUseCase CreateUseCase() => new(_taskRepo.Object);

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_TaskNotFound_ThrowsNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateUseCase().ExecuteAsync(taskId, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_CompletedTask_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem(taskId, "Done task", null, TaskStatus.Done, null, userId, DateTime.UtcNow, null);

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync(task);

        await Assert.ThrowsAsync<DomainException>(
            () => CreateUseCase().ExecuteAsync(taskId, userId));

        _taskRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_PendingTask_DeletesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem(taskId, "Pending task", null, TaskStatus.Pending, null, userId, DateTime.UtcNow, null);

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync(task);
        _taskRepo.Setup(r => r.DeleteAsync(taskId, userId, default)).Returns(System.Threading.Tasks.Task.CompletedTask);

        await CreateUseCase().ExecuteAsync(taskId, userId);

        _taskRepo.Verify(r => r.DeleteAsync(taskId, userId, default), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_InProgressTask_DeletesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem(taskId, "Active task", null, TaskStatus.InProgress, null, userId, DateTime.UtcNow, null);

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync(task);
        _taskRepo.Setup(r => r.DeleteAsync(taskId, userId, default)).Returns(System.Threading.Tasks.Task.CompletedTask);

        await CreateUseCase().ExecuteAsync(taskId, userId);

        _taskRepo.Verify(r => r.DeleteAsync(taskId, userId, default), Times.Once);
    }
}
