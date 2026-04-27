namespace TaskManagementSystem.Application.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
