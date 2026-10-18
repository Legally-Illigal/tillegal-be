using System.Diagnostics;

namespace Tillegal.Infrastructure.Logging;

public sealed class CorrelationIdGenerator
{
    public string Generate() => ActivityTraceId.CreateRandom().ToString();
}
