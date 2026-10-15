namespace Tillegal.Application.Common.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        string routingKey,
        TMessage message,
        CancellationToken cancellationToken)
        where TMessage : notnull;
}
