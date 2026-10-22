namespace Tillegal.Application.Common.Exceptions;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Invalid email/username or password.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
