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
