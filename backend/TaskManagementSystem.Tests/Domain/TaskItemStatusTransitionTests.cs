using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Tests.Domain;

public class TaskItemStatusTransitionTests
{
    [Theory]
    [InlineData(TaskStatus.Pending, TaskStatus.InProgress)]
    [InlineData(TaskStatus.Pending, TaskStatus.Done)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Done)]
    public void TransitionStatus_ValidTransition_UpdatesStatus(TaskStatus from, TaskStatus to)
    {
        var task = BuildTask(status: from);
        task.TransitionStatus(to);
        Assert.Equal(to, task.Status);
    }

    [Theory]
    [InlineData(TaskStatus.Done, TaskStatus.Pending)]
    [InlineData(TaskStatus.Done, TaskStatus.InProgress)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Pending)]
    public void TransitionStatus_InvalidTransition_ThrowsInvalidStatusTransitionException(TaskStatus from, TaskStatus to)
    {
        var task = BuildTask(status: from);
        var ex = Assert.Throws<InvalidStatusTransitionException>(() => task.TransitionStatus(to));
        Assert.Contains(from.ToString(), ex.Message);
        Assert.Contains(to.ToString(), ex.Message);
    }

    [Fact]
    public void TransitionStatus_DoneToInProgress_ThrowsInvalidStatusTransitionException()
    {
        var task = BuildTask(status: TaskStatus.Done);
        Assert.Throws<InvalidStatusTransitionException>(() => task.TransitionStatus(TaskStatus.InProgress));
    }

    [Fact]
    public void TransitionStatus_DoneToPending_ThrowsInvalidStatusTransitionException()
    {
        var task = BuildTask(status: TaskStatus.Done);
        Assert.Throws<InvalidStatusTransitionException>(() => task.TransitionStatus(TaskStatus.Pending));
    }

    private static TaskItem BuildTask(TaskStatus status = TaskStatus.Pending) =>
        new(Guid.NewGuid(), "Valid title", null, status, null, Guid.NewGuid(), DateTime.UtcNow, null);
}
