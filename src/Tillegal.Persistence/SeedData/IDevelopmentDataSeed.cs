namespace Tillegal.Persistence.SeedData;

public interface IDevelopmentDataSeed
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
