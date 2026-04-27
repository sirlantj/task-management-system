using Moq;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.UseCases.Tasks;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Tests.Application;

public class CreateTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();

    private CreateTaskUseCase CreateUseCase() => new(_taskRepo.Object);

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_ValidRequest_CreatesTaskScopedToUser()
    {
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest("Buy milk", null, null);

        _taskRepo.Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), default))
                 .Returns(System.Threading.Tasks.Task.CompletedTask);

        var result = await CreateUseCase().ExecuteAsync(request, userId);

        Assert.Equal("Buy milk", result.Title);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(TaskStatus.Pending.ToString(), result.Status);
        _taskRepo.Verify(r => r.CreateAsync(
            It.Is<TaskItem>(t => t.UserId == userId && t.Title == "Buy milk"), default), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_EmptyTitle_ThrowsDomainException()
    {
        var request = new CreateTaskRequest("", null, null);

        await Assert.ThrowsAsync<TaskManagementSystem.Domain.Exceptions.DomainException>(
            () => CreateUseCase().ExecuteAsync(request, Guid.NewGuid()));
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_PastDueDate_ThrowsDomainException()
    {
        var request = new CreateTaskRequest("Buy milk", null, DateTime.UtcNow.AddDays(-1));

        await Assert.ThrowsAsync<TaskManagementSystem.Domain.Exceptions.DomainException>(
            () => CreateUseCase().ExecuteAsync(request, Guid.NewGuid()));
    }
}
