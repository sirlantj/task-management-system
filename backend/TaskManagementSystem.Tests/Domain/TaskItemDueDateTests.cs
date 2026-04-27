using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Tests.Domain;

public class TaskItemDueDateTests
{
    [Fact]
    public void Constructor_PastDueDate_ThrowsDomainException()
    {
        var past = DateTime.UtcNow.AddDays(-1);
        var ex = Assert.Throws<DomainException>(() => BuildTask(dueDate: past));
        Assert.Contains("past", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_FutureDueDate_CreatesTask()
    {
        var future = DateTime.UtcNow.AddDays(1);
        var task = BuildTask(dueDate: future);
        Assert.Equal(future, task.DueDate);
    }

    [Fact]
    public void Constructor_NullDueDate_CreatesTask()
    {
        var task = BuildTask(dueDate: null);
        Assert.Null(task.DueDate);
    }

    [Fact]
    public void UpdateDueDate_PastDate_ThrowsDomainException()
    {
        var task = BuildTask();
        var past = DateTime.UtcNow.AddDays(-1);
        Assert.Throws<DomainException>(() => task.UpdateDueDate(past));
    }

    [Fact]
    public void UpdateDueDate_FutureDate_UpdatesDueDate()
    {
        var task = BuildTask();
        var future = DateTime.UtcNow.AddDays(5);
        task.UpdateDueDate(future);
        Assert.Equal(future, task.DueDate);
    }

    [Fact]
    public void UpdateDueDate_Null_ClearsDueDate()
    {
        var task = BuildTask(dueDate: DateTime.UtcNow.AddDays(1));
        task.UpdateDueDate(null);
        Assert.Null(task.DueDate);
    }

    private static TaskItem BuildTask(DateTime? dueDate = null) =>
        new(Guid.NewGuid(), "Valid title", null, TaskStatus.Pending, dueDate, Guid.NewGuid(), DateTime.UtcNow, null);
}
