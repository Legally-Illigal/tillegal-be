using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Infrastructure.SystemServices;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
