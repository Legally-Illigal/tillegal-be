using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.SeedData;
using Tillegal.Persistence.Services;

namespace Tillegal.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The PostgreSQL connection string is missing. Configure 'ConnectionStrings:Postgres' using a secret store or environment variable.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());
        services.AddScoped<IDevelopmentDataSeed, ComprehensiveDatabaseDataSeed>();
        services.AddScoped<DevelopmentDatabaseSeeder>();

        return services;
    }
}
