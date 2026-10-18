namespace Tillegal.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
