using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Domain.Exceptions;

namespace TaskManagementSystem.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public TaskItem(
        Guid id,
        string title,
        string? description,
        TaskStatus status,
        DateTime? dueDate,
        Guid userId,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        ValidateTitle(title);

        if (dueDate.HasValue)
            ValidateDueDate(dueDate.Value);

        Id = id;
        Title = title;
        Description = description;
        Status = status;
        DueDate = dueDate;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static TaskItem Reconstitute(
        Guid id,
        string title,
        string? description,
        TaskStatus status,
        DateTime? dueDate,
        Guid userId,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new TaskItem
        {
            Id = id,
            Title = title,
            Description = description,
            Status = status,
            DueDate = dueDate,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    private TaskItem()
    {
        Title = string.Empty;
    }

    public void UpdateTitle(string title)
    {
        ValidateTitle(title);
        Title = title;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateDueDate(DateTime? dueDate)
    {
        if (dueDate.HasValue)
            ValidateDueDate(dueDate.Value);

        DueDate = dueDate;
    }

    public void TransitionStatus(TaskStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
            throw new InvalidStatusTransitionException(Status, newStatus);

        Status = newStatus;
    }

    public bool CanBeDeleted() => Status != TaskStatus.Done;

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title is required and cannot be empty or whitespace.");
    }

    private static void ValidateDueDate(DateTime dueDate)
    {
        if (dueDate.ToUniversalTime() < DateTime.UtcNow)
            throw new DomainException("Due date cannot be in the past.");
    }

    private static bool IsValidTransition(TaskStatus from, TaskStatus to) =>
        (from, to) switch
        {
            (TaskStatus.Pending, TaskStatus.InProgress) => true,
            (TaskStatus.Pending, TaskStatus.Done) => true,
            (TaskStatus.InProgress, TaskStatus.Done) => true,
            _ => false
        };
}
