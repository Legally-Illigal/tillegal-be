using Microsoft.EntityFrameworkCore;
using Tillegal.Persistence.Context;

namespace Tillegal.Persistence.SeedData;

public sealed class DevelopmentDatabaseSeeder(
    AppDbContext dbContext,
    IEnumerable<IDevelopmentDataSeed> dataSeeds)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await ClearAllTablesAsync(cancellationToken);

        foreach (var dataSeed in dataSeeds)
        {
            await dataSeed.SeedAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Truncates all application tables so that seed data is inserted fresh every run.
    /// Uses PostgreSQL TRUNCATE ... CASCADE to handle foreign-key ordering automatically.
    /// </summary>
    private async Task ClearAllTablesAsync(CancellationToken cancellationToken)
    {
        var tableNames = dbContext.Model.GetEntityTypes()
            .Select(t =>
            {
                var schema = t.GetSchema();
                var table = t.GetTableName();
                return table is null
                    ? null
                    : string.IsNullOrEmpty(schema)
                        ? $"\"{table}\""
                        : $"\"{schema}\".\"{table}\"";
            })
            .Where(t => t is not null)
            .Distinct()
            .ToList();

        if (tableNames.Count == 0)
        {
            return;
        }

        var joined = string.Join(", ", tableNames);

        // Table names come from EF Core model metadata, not user input — safe from SQL injection.
#pragma warning disable EF1002
        await dbContext.Database.ExecuteSqlRawAsync(
            $"TRUNCATE TABLE {joined} CASCADE",
            cancellationToken);
#pragma warning restore EF1002
    }
}
