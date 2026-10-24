using System.Security.Cryptography;
using System.Text.Json;
using MediatR;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Services;

namespace Tillegal.Application.Features.Auth.Commands.ResendOtp;

public sealed record ResendOtpCommand(string Email) : IRequest<ResendOtpResult>;

public sealed record ResendOtpResult(string Message, int ExpirationMinutes);
