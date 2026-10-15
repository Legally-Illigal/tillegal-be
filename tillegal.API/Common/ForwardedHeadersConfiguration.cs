using System.Net;

using Microsoft.AspNetCore.HttpOverrides;

namespace Tillegal.API.Common;

public static class ForwardedHeadersConfiguration
{
    public static void ConfigureKnownProxies(
        ForwardedHeadersOptions options,
        IConfiguration configuration)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
            | ForwardedHeaders.XForwardedProto;

        var knownProxies = configuration
            .GetSection("ForwardedHeaders:KnownProxies")
            .Get<string[]>() ?? [];

        foreach (var proxy in knownProxies)
        {
            if (!IPAddress.TryParse(proxy, out IPAddress? address))
            {
                throw new InvalidOperationException(
                    $"ForwardedHeaders:KnownProxies contains an invalid IP address: {proxy}");
            }

            options.KnownProxies.Add(address);
        }
    }
}
