using Microsoft.EntityFrameworkCore;
using Tillegal.Persistence.Context;

namespace Tillegal.Persistence.SeedData;

public sealed class DevelopmentDatabaseSeeder(
    AppDbContext dbContext,
    IEnumerable<IDevelopmentDataSeed> dataSeeds)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        foreach (var dataSeed in dataSeeds)
        {
            await dataSeed.SeedAsync(cancellationToken);
        }
    }
}
