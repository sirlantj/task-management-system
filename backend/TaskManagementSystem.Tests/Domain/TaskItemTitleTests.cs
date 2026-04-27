using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Tests.Domain;

public class TaskItemTitleTests
{
    [Fact]
    public void Constructor_EmptyTitle_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => BuildTask(title: ""));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_WhiteSpaceTitle_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => BuildTask(title: "   "));
    }

    [Fact]
    public void UpdateTitle_EmptyTitle_ThrowsDomainException()
    {
        var task = BuildTask();
        Assert.Throws<DomainException>(() => task.UpdateTitle(""));
    }

    [Fact]
    public void UpdateTitle_WhiteSpaceTitle_ThrowsDomainException()
    {
        var task = BuildTask();
        Assert.Throws<DomainException>(() => task.UpdateTitle("  "));
    }

    [Fact]
    public void UpdateTitle_ValidTitle_UpdatesTitle()
    {
        var task = BuildTask();
        task.UpdateTitle("New title");
        Assert.Equal("New title", task.Title);
    }

    private static TaskItem BuildTask(string title = "Valid title") =>
        new(Guid.NewGuid(), title, null, TaskStatus.Pending, null, Guid.NewGuid(), DateTime.UtcNow, null);
}
