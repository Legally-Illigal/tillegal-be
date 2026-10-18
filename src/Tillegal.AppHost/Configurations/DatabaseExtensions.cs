using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tillegal.AppHost.Options;
using Tillegal.Persistence;

namespace Tillegal.AppHost.Configurations;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PostgreSQL connection string is missing. Configure 'ConnectionStrings:Postgres' using a secret store or environment variable.");
        }

        services.AddOptions<DatabaseOptions>()
            .Configure(options => options.ConnectionString = connectionString)
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "PostgreSQL connection string is required.")
            .ValidateOnStart();

        return services.AddPersistence(configuration);
    }
}
