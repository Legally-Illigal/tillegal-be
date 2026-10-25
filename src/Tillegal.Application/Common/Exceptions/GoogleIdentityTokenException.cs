namespace Tillegal.Application.Common.Exceptions;

public sealed class GoogleIdentityTokenException(string message, Exception innerException)
    : Exception(message, innerException);
