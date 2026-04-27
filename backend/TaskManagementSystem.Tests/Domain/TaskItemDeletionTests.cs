using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Tests.Domain;

public class TaskItemDeletionTests
{
    [Theory]
    [InlineData(TaskStatus.Pending)]
    [InlineData(TaskStatus.InProgress)]
    public void CanBeDeleted_NonDoneStatus_ReturnsTrue(TaskStatus status)
    {
        var task = BuildTask(status);
        Assert.True(task.CanBeDeleted());
    }

    [Fact]
    public void CanBeDeleted_DoneStatus_ReturnsFalse()
    {
        var task = BuildTask(TaskStatus.Done);
        Assert.False(task.CanBeDeleted());
    }

    private static TaskItem BuildTask(TaskStatus status) =>
        new(Guid.NewGuid(), "Valid title", null, status, null, Guid.NewGuid(), DateTime.UtcNow, null);
}
