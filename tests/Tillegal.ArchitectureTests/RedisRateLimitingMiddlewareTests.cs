using System.Net;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tillegal.Api.Middlewares;
using Tillegal.Api.Options;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Xunit;

namespace Tillegal.ArchitectureTests;

public sealed class RedisRateLimitingMiddlewareTests
{
    private sealed class FakeRateLimitService : IRateLimitService
    {
        public string? LastPartitionKey { get; private set; }
        public int LastPermitLimit { get; private set; }
        public TimeSpan LastWindow { get; private set; }
        public int CallCount { get; private set; }
        public bool ShouldThrow { get; set; }

        public RateLimitCheckResult ResultToReturn { get; set; } = new(
            IsAllowed: true,
            CurrentCount: 1,
            Remaining: 99,
            RetryAfter: TimeSpan.Zero,
            ResetAt: DateTimeOffset.UtcNow.AddSeconds(60));

        public Task<RateLimitCheckResult> CheckRateLimitAsync(
            string partitionKey,
            int permitLimit,
            TimeSpan window,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastPartitionKey = partitionKey;
            LastPermitLimit = permitLimit;
            LastWindow = window;

            if (ShouldThrow)
            {
                throw new InvalidOperationException("Redis connection error");
            }

            return Task.FromResult(ResultToReturn);
        }
    }

    private readonly RateLimitingOptions _defaultOptions = new()
    {
        Global = new RateLimitPolicyOptions { PermitLimit = 100, WindowSeconds = 60 },
        Login = new RateLimitPolicyOptions { PermitLimit = 10, WindowSeconds = 60 }
    };

    [Theory]
    [InlineData("/healthz")]
    [InlineData("/readyz")]
    [InlineData("/swagger/index.html")]
    [InlineData("/favicon.ico")]
    public async Task InvokeAsync_NonApiRoutes_ShouldBypassRateLimiter(string path)
    {
        // Arrange
        var fakeService = new FakeRateLimitService();
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RedisRateLimitingMiddleware>.Instance,
            Options.Create(_defaultOptions));

        var context = new DefaultHttpContext();
        context.Request.Path = path;

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.CallCount.Should().Be(0);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_OptionsPreflightRequest_ShouldBypassRateLimiter()
    {
        // Arrange
        var fakeService = new FakeRateLimitService();
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RedisRateLimitingMiddleware>.Instance,
            Options.Create(_defaultOptions));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Options;
        context.Request.Path = "/api/conversations";

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.CallCount.Should().Be(0);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_GlobalEndpoint_UnauthenticatedUser_ShouldPartitionByIpAndSetHeaders()
    {
        // Arrange
        var fakeService = new FakeRateLimitService();
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RedisRateLimitingMiddleware>.Instance,
            Options.Create(_defaultOptions));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/conversations";
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.CallCount.Should().Be(1);
        fakeService.LastPartitionKey.Should().Be("global:ip:192.168.1.100");
        fakeService.LastPermitLimit.Should().Be(100);
        fakeService.LastWindow.Should().Be(TimeSpan.FromSeconds(60));
        nextCalled.Should().BeTrue();

        context.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("100");
        context.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("99");
        context.Response.Headers.ContainsKey("X-RateLimit-Reset").Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_GlobalEndpoint_AuthenticatedUser_ShouldPartitionByUserId()
    {
        // Arrange
        var fakeService = new FakeRateLimitService();
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RedisRateLimitingMiddleware>.Instance,
            Options.Create(_defaultOptions));

        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/conversations";
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.CallCount.Should().Be(1);
        fakeService.LastPartitionKey.Should().Be($"global:user:{userId}");
        fakeService.LastPermitLimit.Should().Be(100);
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/login/google")]
    public async Task InvokeAsync_LoginEndpoints_ShouldUseLoginPolicyAndPartitionByIp(string loginPath)
    {
        // Arrange
        var fakeService = new FakeRateLimitService();
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RedisRateLimitingMiddleware>.Instance,
            Options.Create(_defaultOptions));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = loginPath;
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.5");

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.CallCount.Should().Be(1);
        fakeService.LastPartitionKey.Should().Be("login:10.0.0.5");
        fakeService.LastPermitLimit.Should().Be(10);
        fakeService.LastWindow.Should().Be(TimeSpan.FromSeconds(60));
        nextCalled.Should().BeTrue();
        context.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("10");
    }

    [Fact]
    public async Task InvokeAsync_WhenRateLimitExceeded_ShouldReturn429AndNotCallNext()
    {
        // Arrange
        var fakeService = new FakeRateLimitService
        {
            ResultToReturn = new RateLimitCheckResult(
                IsAllowed: false,
                CurrentCount: 11,
                Remaining: 0,
                RetryAfter: TimeSpan.FromSeconds(42),
                ResetAt: DateTimeOffset.UtcNow.AddSeconds(42))
        };

        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RedisRateLimitingMiddleware>.Instance,
            Options.Create(_defaultOptions));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/auth/login";
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.5");
        context.TraceIdentifier = "test-correlation-id";

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        context.Response.Headers["Retry-After"].ToString().Should().Be("42");
        context.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("10");
        context.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("0");
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be("test-correlation-id");
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(StatusCodes.Status429TooManyRequests);
        problem.Title.Should().Be("Too Many Requests");
        problem.Detail.Should().Contain("42 seconds");
    }

    [Fact]
    public async Task InvokeAsync_WhenServiceThrows_ShouldFailOpenAndCallNext()
    {
        // Arrange
        var fakeService = new FakeRateLimitService { ShouldThrow = true };
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RedisRateLimitingMiddleware>.Instance,
            Options.Create(_defaultOptions));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/conversations";
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        await middleware.InvokeAsync(context, fakeService);

        // Assert
        fakeService.CallCount.Should().Be(1);
        nextCalled.Should().BeTrue();
    }
}
