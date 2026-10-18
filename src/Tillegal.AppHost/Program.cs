using Tillegal.Infrastructure;
using Tillegal.Persistence;
using Tillegal.Persistence.SeedData;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
