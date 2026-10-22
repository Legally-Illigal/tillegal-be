using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Tillegal.Api.Middlewares;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Xunit;

namespace Tillegal.ArchitectureTests;

public sealed class AuditLogMiddlewareTests
{
    private sealed class FakeAuditLogService : IAuditLogService
    {
        public List<AuditLogEntry> LoggedEntries { get; } = new();

        public ValueTask EnqueueAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
        {
            LoggedEntries.Add(entry);
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task InvokeAsync_NonAuditableMethod_ShouldNotEnqueueAuditLog()
    {
        // Arrange
        var fakeService = new FakeAuditLogService();
        var middleware = new AuditLogMiddleware(_ => Task.CompletedTask, NullLogger<AuditLogMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/conversations";

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.LoggedEntries.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_AuditableMethod_ShouldEnqueueAuditLogWithCorrectDetails()
    {
        // Arrange
        var fakeService = new FakeAuditLogService();
        var middleware = new AuditLogMiddleware(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status201Created;
            return Task.CompletedTask;
        }, NullLogger<AuditLogMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/v1/conversations";
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.LoggedEntries.Should().HaveCount(1);
        var capturedEntry = fakeService.LoggedEntries[0];
        capturedEntry.EventType.Should().Be("HTTP_REQUEST");
        capturedEntry.ActorUserId.Should().Be(userId);
        capturedEntry.ActorRole.Should().Be("User");
        capturedEntry.IpAddress.Should().Be("127.0.0.1");
        capturedEntry.Action.Should().Be("POST /api/v1/conversations");
        capturedEntry.Status.Should().Be("SUCCESS");
        capturedEntry.Description.Should().Contain("status 201");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldLogFailedStatusAndRethrow()
    {
        // Arrange
        var fakeService = new FakeAuditLogService();
        var middleware = new AuditLogMiddleware(_ => throw new InvalidOperationException("Something went wrong"), NullLogger<AuditLogMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Delete;
        context.Request.Path = "/api/v1/conversations/123";

        // Act
        var act = () => middleware.InvokeAsync(context, fakeService);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Something went wrong");
        fakeService.LoggedEntries.Should().HaveCount(1);
        var capturedEntry = fakeService.LoggedEntries[0];
        capturedEntry.Status.Should().Be("FAILED");
        capturedEntry.Action.Should().Be("DELETE /api/v1/conversations/123");
        capturedEntry.Description.Should().Contain("status 500");
    }
}
