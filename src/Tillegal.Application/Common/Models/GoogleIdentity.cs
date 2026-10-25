namespace Tillegal.Application.Common.Models;

public sealed record GoogleIdentity(string Subject, string Email, string FullName, bool EmailVerified);
