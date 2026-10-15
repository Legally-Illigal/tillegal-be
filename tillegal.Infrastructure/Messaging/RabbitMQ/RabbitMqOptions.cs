namespace Tillegal.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqOptions
{
    public string Uri { get; init; } = string.Empty;
    public string ExchangeName { get; init; } = "tillegal.events";
}
