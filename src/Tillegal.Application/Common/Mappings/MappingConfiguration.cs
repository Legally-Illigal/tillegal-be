using Mapster;

namespace Tillegal.Application.Common.Mappings;

public static class MappingConfiguration
{
    public static TypeAdapterConfig Create()
    {
        var configuration = new TypeAdapterConfig();
        configuration.Scan(typeof(MappingConfiguration).Assembly);
        return configuration;
    }
}
