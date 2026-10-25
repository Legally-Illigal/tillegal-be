using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Tillegal.Infrastructure.Options;

namespace Tillegal.Infrastructure.ExternalServices.Auth;

public sealed class GoogleIdentityTokenValidator(IOptions<GoogleAuthOptions> options)
    : IGoogleIdentityTokenValidator
{
    public async Task<GoogleIdentity> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [options.Value.ClientId]
                });

            return new GoogleIdentity(
                payload.Subject ?? string.Empty,
                payload.Email ?? string.Empty,
                payload.Name ?? string.Empty,
                payload.EmailVerified);
        }
        catch (InvalidJwtException exception)
        {
            throw new GoogleIdentityTokenException("The Google ID token is invalid or expired.", exception);
        }
    }
}
