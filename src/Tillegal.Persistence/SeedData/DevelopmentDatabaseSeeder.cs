namespace Tillegal.Persistence.SeedData;

public sealed class DevelopmentDatabaseSeeder(IEnumerable<IDevelopmentDataSeed> dataSeeds)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var dataSeed in dataSeeds)
        {
            await dataSeed.SeedAsync(cancellationToken);
        }
    }
}
