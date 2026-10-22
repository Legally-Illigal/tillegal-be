using Microsoft.EntityFrameworkCore;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.SeedData;

namespace Tillegal.AppHost.Extensions;

public static class MigrationExtensions
{
    public static async Task<WebApplication> ApplyDevelopmentDatabaseChangesAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        // Seeder only executes on Development or Staging, never on Production
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Staging"))
        {
            return app;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);

        return app;
    }
}
