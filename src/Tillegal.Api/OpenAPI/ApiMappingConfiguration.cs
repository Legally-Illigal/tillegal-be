using Mapster;

namespace Tillegal.Api.OpenAPI;

public static class ApiMappingConfiguration
{
    public static TypeAdapterConfig Create()
    {
        var configuration = new TypeAdapterConfig();
        configuration.Scan(
            typeof(Tillegal.Application.DependencyInjection).Assembly,
            typeof(ApiMappingConfiguration).Assembly);
        return configuration;
    }
}
