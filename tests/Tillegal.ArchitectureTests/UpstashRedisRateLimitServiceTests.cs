using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tillegal.Infrastructure.ExternalServices.RateLimiting;
using Tillegal.Infrastructure.Options;
using Xunit;

namespace Tillegal.ArchitectureTests;

public sealed class UpstashRedisRateLimitServiceTests
{
    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        public List<string> RecordedRequests { get; } = new();
        public Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; set; } = _ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\": 1}")
            };

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                var content = await request.Content.ReadAsStringAsync(cancellationToken);
                RecordedRequests.Add(content);
            }

            return ResponseFactory(request);
        }
    }

    private readonly IOptions<RedisOptions> _options = Options.Create(new RedisOptions
    {
        RedisRestUrl = "https://test.upstash.io",
        RedisRestToken = "test_token"
    });

    [Fact]
    public async Task CheckRateLimitAsync_WhenCountUnderLimit_ShouldReturnAllowed()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\": 3}")
            }
        };
        var httpClient = new HttpClient(handler);
        var service = new UpstashRedisRateLimitService(
            httpClient,
            _options,
            NullLogger<UpstashRedisRateLimitService>.Instance);

        // Act
        var result = await service.CheckRateLimitAsync("test-user", 10, TimeSpan.FromMinutes(1));

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.CurrentCount.Should().Be(3);
        result.Remaining.Should().Be(7);
        result.RetryAfter.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task CheckRateLimitAsync_WhenCountExceedsLimit_ShouldReturnNotAllowed()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\": 15}")
            }
        };
        var httpClient = new HttpClient(handler);
        var service = new UpstashRedisRateLimitService(
            httpClient,
            _options,
            NullLogger<UpstashRedisRateLimitService>.Instance);

        // Act
        var result = await service.CheckRateLimitAsync("test-ip", 10, TimeSpan.FromMinutes(1));

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.CurrentCount.Should().Be(15);
        result.Remaining.Should().Be(0);
        result.RetryAfter.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task CheckRateLimitAsync_WhenCountIsOne_ShouldSendExpireCommand()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\": 1}")
            }
        };
        var httpClient = new HttpClient(handler);
        var service = new UpstashRedisRateLimitService(
            httpClient,
            _options,
            NullLogger<UpstashRedisRateLimitService>.Instance);

        // Act
        await service.CheckRateLimitAsync("key1", 10, TimeSpan.FromMinutes(1));

        // Assert: 2 HTTP calls (INCR, then EXPIRE)
        handler.RecordedRequests.Should().HaveCount(2);
        handler.RecordedRequests[0].Should().Contain("INCR");
        handler.RecordedRequests[1].Should().Contain("EXPIRE");
    }

    [Fact]
    public async Task CheckRateLimitAsync_WhenHttpCallFails_ShouldFailOpen()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        };
        var httpClient = new HttpClient(handler);
        var service = new UpstashRedisRateLimitService(
            httpClient,
            _options,
            NullLogger<UpstashRedisRateLimitService>.Instance);

        // Act
        var result = await service.CheckRateLimitAsync("failing-key", 10, TimeSpan.FromMinutes(1));

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.CurrentCount.Should().Be(1);
        result.Remaining.Should().Be(9);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CheckRateLimitAsync_InvalidPartitionKey_ShouldThrowArgumentException(string invalidKey)
    {
        var httpClient = new HttpClient(new TestHttpMessageHandler());
        var service = new UpstashRedisRateLimitService(
            httpClient,
            _options,
            NullLogger<UpstashRedisRateLimitService>.Instance);

        var act = () => service.CheckRateLimitAsync(invalidKey, 10, TimeSpan.FromMinutes(1));
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CheckRateLimitAsync_InvalidPermitLimit_ShouldThrowArgumentOutOfRangeException()
    {
        var httpClient = new HttpClient(new TestHttpMessageHandler());
        var service = new UpstashRedisRateLimitService(
            httpClient,
            _options,
            NullLogger<UpstashRedisRateLimitService>.Instance);

        var act = () => service.CheckRateLimitAsync("valid-key", 0, TimeSpan.FromMinutes(1));
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task CheckRateLimitAsync_InvalidWindow_ShouldThrowArgumentOutOfRangeException()
    {
        var httpClient = new HttpClient(new TestHttpMessageHandler());
        var service = new UpstashRedisRateLimitService(
            httpClient,
            _options,
            NullLogger<UpstashRedisRateLimitService>.Instance);

        var act = () => service.CheckRateLimitAsync("valid-key", 10, TimeSpan.Zero);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
