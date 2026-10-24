using MediatR;

namespace Tillegal.Application.Features.Auth.Commands.VerifyRegistration;

public sealed record VerifyRegistrationCommand(string Email, string Otp) : IRequest<VerifyRegistrationResult>;

public sealed record VerifyRegistrationResult(Guid UserId, string Email, string FullName, string Message);
