namespace Tillegal.Api.Contracts.Requests;

public sealed record LoginRequest(string EmailOrUserName, string Password);
