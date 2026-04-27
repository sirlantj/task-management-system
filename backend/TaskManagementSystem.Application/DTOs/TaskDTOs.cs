using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Application.DTOs;

public record CreateTaskRequest(string Title, string? Description, DateTime? DueDate);

public record UpdateTaskRequest(string Title, string? Description, DateTime? DueDate, string? Status);

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime? DueDate,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

internal static class TaskMapper
{
    public static TaskResponse ToResponse(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status.ToString(),
            task.DueDate, task.UserId, task.CreatedAt, task.UpdatedAt);
}
