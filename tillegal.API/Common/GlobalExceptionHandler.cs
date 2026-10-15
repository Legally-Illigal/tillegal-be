using System.Net;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

using Npgsql;

using Tillegal.API.Models;
using Tillegal.Domain.Exceptions;

namespace Tillegal.API.Common
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            (HttpStatusCode statusCode, string[]? messages) = exception switch
            {
                ValidationException validationException =>
                (
                    HttpStatusCode.BadRequest,
                    validationException.Errors
                        .Select(error => error.ErrorMessage)
                        .ToArray()),

                NotFoundException =>
                    (HttpStatusCode.NotFound,
                    new[] { exception.Message }),

                ConflictException =>
                    (HttpStatusCode.Conflict,
                    new[] { exception.Message }),

                ArgumentException or InvalidOperationException =>
                    (HttpStatusCode.BadRequest,
                    new[] { exception.Message }),

                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized,
                    new[] { exception.Message }),

                PostgresException postgresException
                    when postgresException.SqlState == PostgresErrorCodes.UniqueViolation =>
                    (HttpStatusCode.Conflict,
                    new[]
                    {
                        "Dữ liệu bị trùng lặp trong hệ thống.",
                    }),

                DbUpdateException { InnerException: PostgresException postgresException }
                    when postgresException.SqlState == PostgresErrorCodes.UniqueViolation =>
                    (HttpStatusCode.Conflict,
                    new[]
                    {
                        "Dữ liệu bị trùng lặp trong hệ thống.",
                    }),

                _ =>
                    (HttpStatusCode.InternalServerError,
                    new[]
                    {
                        "An unexpected error occurred.",
                    }),
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Path}",
                    httpContext.Request.Path);
            }

            var response =
                ApiResponse.Fail(
                    statusCode,
                    messages);

            httpContext.Response.StatusCode =
                (int)statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }
    }
}
