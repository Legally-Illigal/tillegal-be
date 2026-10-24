using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.SeedData;
using Xunit;

namespace Tillegal.ArchitectureTests;

public sealed class SeederVerificationTests
{
    private static AppDbContext CreateDbContext()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "tillegal-be.slnx")))
        {
            dir = dir.Parent;
        }

        if (dir == null)
        {
            throw new DirectoryNotFoundException("Could not locate solution root directory.");
        }

        var configPath = Path.Combine(dir.FullName, "src", "Tillegal.AppHost");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(configPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Seed_Should_HaveSeededExpectedCounts_And_BeIdempotent()
    {
        await using var dbContext = CreateDbContext();

        // --- BƯỚC THÊM MỚI: Reset DB và chạy lại Seed trước khi Test ---
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        var initialSeeder = new ComprehensiveDatabaseDataSeed(dbContext);
        await initialSeeder.SeedAsync();
        // -------------------------------------------------------------

        // 1. Verify Roles (3 records)
        var roles = await dbContext.Roles.ToListAsync();
        roles.Should().HaveCount(3);
        roles.Select(r => r.Name).Should().Contain(["Admin", "User", "Lawer"]);

        // 2. Verify Users (10 records: 2 Admin, 5 User, 3 Lawer)
        var adminRole = roles.First(r => r.Name == "Admin");
        var userRole = roles.First(r => r.Name == "User");
        var lawerRole = roles.First(r => r.Name == "Lawer");

        var users = await dbContext.Users.ToListAsync();
        users.Should().HaveCount(10);
        users.Count(u => u.RoleId == adminRole.Id).Should().Be(2);
        users.Count(u => u.RoleId == userRole.Id).Should().Be(5);
        users.Count(u => u.RoleId == lawerRole.Id).Should().Be(3);
        users.Should().AllSatisfy(u => u.PasswordHash.Should().NotBeNullOrWhiteSpace());

        // 3. Verify Conversations (15 records)
        var conversations = await dbContext.Conversations.ToListAsync();
        conversations.Should().HaveCount(15);

        // 4. Verify ConversationMessages (60 records)
        var messages = await dbContext.ConversationMessages.ToListAsync();
        messages.Should().HaveCount(60);

        var assistantMessages = messages.Where(m => m.Role == "assistant").ToList();
        assistantMessages.Should().HaveCount(30);
        assistantMessages.Should().AllSatisfy(m =>
        {
            m.Citations.Should().NotBeNull();
            m.Content.Should().Contain("[^1]");
            var root = m.Citations!.RootElement;
            root.ValueKind.Should().Be(JsonValueKind.Array);
            root.GetArrayLength().Should().BeGreaterThan(0);
        });

        // 5. Verify UserAttachments (8 records)
        var attachments = await dbContext.UserAttachments.ToListAsync();
        attachments.Should().HaveCount(8);
        attachments.Select(a => a.ProcessingStatus).Distinct().Should().Contain(["Pending", "Processing", "Completed", "Failed"]);

        // 6. Verify AiEvaluationLogs (15 records)
        var evaluations = await dbContext.AiEvaluationLogs.ToListAsync();
        evaluations.Should().HaveCount(15);
        evaluations.Should().AllSatisfy(e =>
        {
            e.Rating.Should().BeInRange(1, 5);
            e.IsHelpful.Should().NotBeNull();
            e.FeedbackComment.Should().NotBeNullOrWhiteSpace();
        });

        // 7. Verify AuditLogs (30 records)
        var auditLogs = await dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().HaveCount(30);
        auditLogs.Select(a => a.Action).Distinct().Should().Contain(["LOGIN_SUCCESS", "CREATE_CONVERSATION", "UPLOAD_ATTACHMENT", "EVALUATE_MESSAGE"]);

        // 8. Test Idempotency: execute seed again
        var seeder = new ComprehensiveDatabaseDataSeed(dbContext);
        var act = async () => await seeder.SeedAsync();
        await act.Should().NotThrowAsync();

        // Verify counts remain unchanged
        (await dbContext.Roles.CountAsync()).Should().Be(3);
        (await dbContext.Users.CountAsync()).Should().Be(10);
        (await dbContext.Conversations.CountAsync()).Should().Be(15);
        (await dbContext.ConversationMessages.CountAsync()).Should().Be(60);
        (await dbContext.UserAttachments.CountAsync()).Should().Be(8);
        (await dbContext.AiEvaluationLogs.CountAsync()).Should().Be(15);
        (await dbContext.AuditLogs.CountAsync()).Should().Be(30);
    }
}
