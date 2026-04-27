using Moq;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Exceptions;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.UseCases.Tasks;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Tests.Application;

public class UpdateTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();

    private UpdateTaskUseCase CreateUseCase() => new(_taskRepo.Object);

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_TaskNotFound_ThrowsNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateUseCase().ExecuteAsync(taskId, userId, new UpdateTaskRequest("Title", null, null, null)));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_ValidStatusTransition_UpdatesStatus()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem(taskId, "Task", null, TaskStatus.Pending, null, userId, DateTime.UtcNow, null);

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync(task);
        _taskRepo.Setup(r => r.UpdateAsync(task, default)).Returns(System.Threading.Tasks.Task.CompletedTask);

        var result = await CreateUseCase().ExecuteAsync(taskId, userId,
            new UpdateTaskRequest("Task", null, null, "InProgress"));

        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_InvalidStatusTransition_ThrowsInvalidStatusTransitionException()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem(taskId, "Task", null, TaskStatus.Done, null, userId, DateTime.UtcNow, null);

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync(task);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => CreateUseCase().ExecuteAsync(taskId, userId,
                new UpdateTaskRequest("Task", null, null, "Pending")));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_NoStatusProvided_DoesNotTransition()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem(taskId, "Old title", null, TaskStatus.Pending, null, userId, DateTime.UtcNow, null);

        _taskRepo.Setup(r => r.GetByIdAsync(taskId, userId, default)).ReturnsAsync(task);
        _taskRepo.Setup(r => r.UpdateAsync(task, default)).Returns(System.Threading.Tasks.Task.CompletedTask);

        var result = await CreateUseCase().ExecuteAsync(taskId, userId,
            new UpdateTaskRequest("New title", null, null, null));

        Assert.Equal("New title", result.Title);
        Assert.Equal(TaskStatus.Pending.ToString(), result.Status);
    }
}
