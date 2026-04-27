using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(TaskStatus from, TaskStatus to)
        : base($"Cannot transition task status from '{from}' to '{to}'.") { }
}
