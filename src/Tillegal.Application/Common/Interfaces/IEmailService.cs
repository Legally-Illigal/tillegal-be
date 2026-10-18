namespace Tillegal.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
