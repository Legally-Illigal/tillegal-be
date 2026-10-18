namespace Tillegal.Domain.Exceptions;

public sealed class UnauthorizedAccessException : DomainException
{
    public UnauthorizedAccessException(string message)
        : base(message)
    {
    }
}
