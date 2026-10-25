using Tillegal.Application.Common.Models;

namespace Tillegal.Application.Common.Interfaces;

public interface IGoogleIdentityTokenValidator
{
    Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
