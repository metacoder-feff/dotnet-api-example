using Microsoft.Extensions.DependencyInjection.Extensions;
using static Example.Utils.ThrowHelper;

namespace Example.Utils;

public static class ServiceCollectionExtention
{
    public static void TryAddTimeProvider(this IServiceCollection services)
    {
        services.TryAddSingleton((_) => TimeProvider.System);
    }

    public static void TryAddRandom(this IServiceCollection services)
    {
        services.TryAddSingleton((_) => Random.Shared);
    }
    
    public static IServiceCollection TryReplaceSingleton<TService>(this IServiceCollection services, TService instance)
        where TService : class
    {
        var srcType = typeof(TService);
        var oldD = services.SingleOrDefault(d => d.ServiceType == srcType);
        if (oldD == null)
            return services;

        Assert(oldD.Lifetime == ServiceLifetime.Singleton);

        var sdNew = new ServiceDescriptor(srcType, instance);
        services.Replace(sdNew);

        return services;
    }

    /// <summary>
    /// Cloud-compatible logging:
    /// - stdout
    /// - json-lines
    /// - timestamp
    /// Also log scopes
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddStdCloudLogging(this IServiceCollection services)
    {
        services.AddLogging(
            l => l.AddJsonConsole(
                j =>
                {
                    j.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.ffffffZ";
                    j.UseUtcTimestamp = true;
                    j.IncludeScopes = true;
                    j.UseUtcTimestamp = true;
                    j.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
                    {
                        Indented = false
                    };
                }
            )
        );
        return services;
    }
}