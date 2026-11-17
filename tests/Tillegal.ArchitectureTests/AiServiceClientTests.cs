using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Infrastructure.ExternalServices.Ai;
using Tillegal.Infrastructure.ExternalServices.Jobs;
using Tillegal.Infrastructure.Options;
using Xunit;

namespace Tillegal.ArchitectureTests;

public sealed class AiServiceClientTests
{
    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }
        public string? CapturedContent { get; private set; }
        public Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; set; } = _ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\": \"job-abc-456\"}", System.Text.Encoding.UTF8, "application/json")
            };

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            if (request.Content is not null)
            {
                CapturedContent = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return ResponseFactory(request);
        }
    }

    [Fact]
    public async Task TriggerCrawlAsync_ShouldSendPostRequestWithCorrectPayload_AndReturnJobId()
    {
        // Arrange
        var handler = new TestHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = Microsoft.Extensions.Options.Options.Create(new AiServiceOptions
        {
            BaseUrl = "http://127.0.0.1:8000"
        });
        var service = new AiServiceClient(httpClient, options, NullLogger<AiServiceClient>.Instance);
        var targetDate = new DateOnly(2026, 11, 16);

        // Act
        var result = await service.TriggerCrawlAsync(targetDate);

        // Assert
        result.Should().Be("job-abc-456");
        handler.CapturedRequest.Should().NotBeNull();
        handler.CapturedRequest!.Method.Should().Be(HttpMethod.Post);
        handler.CapturedRequest.RequestUri!.ToString().Should().Be("http://127.0.0.1:8000/ingestion/vbpl/crawl");
        handler.CapturedContent.Should().Contain("\"crawl_date\":\"2026-11-16\"");
    }

    [Fact]
    public async Task TriggerCrawlAsync_WhenApiReturnsError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        };
        var httpClient = new HttpClient(handler);
        var options = Microsoft.Extensions.Options.Options.Create(new AiServiceOptions
        {
            BaseUrl = "http://127.0.0.1:8000"
        });
        var service = new AiServiceClient(httpClient, options, NullLogger<AiServiceClient>.Instance);
        var targetDate = new DateOnly(2026, 11, 16);

        // Act & Assert
        var act = async () => await service.TriggerCrawlAsync(targetDate);
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}

public sealed class DailyCrawlUpdateTaskTests
{
    private sealed class FakeDateTimeProvider(DateTimeOffset utcNow) : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => utcNow;
    }

    private sealed class FakeAiServiceClient : IAiServiceClient
    {
        public DateOnly? CapturedDate { get; private set; }
        public string? ReturnJobId { get; set; } = "job-scheduled-789";
        public bool ShouldThrow { get; set; }

        public Task<string?> TriggerCrawlAsync(DateOnly crawlDate, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new HttpRequestException("Downstream crawler failure");
            }

            CapturedDate = crawlDate;
            return Task.FromResult(ReturnJobId);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateYesterdayDateInConfiguredTimeZone_AndCallAiService()
    {
        // Arrange: 2026-11-17 01:00 UTC = 2026-11-17 08:00 (Asia/Ho_Chi_Minh). Yesterday should be 2026-11-16.
        var utcNow = new DateTimeOffset(2026, 11, 17, 1, 0, 0, TimeSpan.Zero);
        var dateTimeProvider = new FakeDateTimeProvider(utcNow);
        var aiServiceClient = new FakeAiServiceClient();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackgroundJobs:TimeZone"] = "Asia/Ho_Chi_Minh"
            })
            .Build();

        var task = new DailyCrawlUpdateTask(
            aiServiceClient,
            dateTimeProvider,
            configuration,
            NullLogger<DailyCrawlUpdateTask>.Instance);

        // Act
        await task.ExecuteAsync();

        // Assert
        aiServiceClient.CapturedDate.Should().Be(new DateOnly(2026, 11, 16));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCrossingMidnight_CalculatesCorrectPreviousDay()
    {
        // Arrange: 2026-11-16 17:05 UTC = 2026-11-17 00:05 (Asia/Ho_Chi_Minh) -> midnight job.
        // Yesterday in local time should be 2026-11-16.
        var utcNow = new DateTimeOffset(2026, 11, 16, 17, 5, 0, TimeSpan.Zero);
        var dateTimeProvider = new FakeDateTimeProvider(utcNow);
        var aiServiceClient = new FakeAiServiceClient();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackgroundJobs:TimeZone"] = "Asia/Ho_Chi_Minh"
            })
            .Build();

        var task = new DailyCrawlUpdateTask(
            aiServiceClient,
            dateTimeProvider,
            configuration,
            NullLogger<DailyCrawlUpdateTask>.Instance);

        // Act
        await task.ExecuteAsync();

        // Assert
        aiServiceClient.CapturedDate.Should().Be(new DateOnly(2026, 11, 16));
    }

    [Fact]
    public async Task ExecuteAsync_WhenAiServiceFails_ShouldRethrowException()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 11, 17, 0, 0, 0, TimeSpan.Zero);
        var dateTimeProvider = new FakeDateTimeProvider(utcNow);
        var aiServiceClient = new FakeAiServiceClient { ShouldThrow = true };
        var configuration = new ConfigurationBuilder().Build();

        var task = new DailyCrawlUpdateTask(
            aiServiceClient,
            dateTimeProvider,
            configuration,
            NullLogger<DailyCrawlUpdateTask>.Instance);

        // Act & Assert
        var act = async () => await task.ExecuteAsync();
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
