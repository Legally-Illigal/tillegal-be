namespace Tillegal.Domain.Exceptions;

public sealed class ForbiddenAccessException : DomainException
{
    public ForbiddenAccessException(string message = "Access to this resource is forbidden.")
        : base(message)
    {
    }
}
