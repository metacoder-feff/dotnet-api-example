using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;
using FEFF.Extentions.DependencyInjection;

public static class DependencyInjectionExtentions
{
//TODO: keyed
    public static IServiceCollection AddInterfaceForImplementation<TService, TImplementation>(this IServiceCollection services)
        where TImplementation     : class, TService
        where TService : class
    {
//TODO: add with same ServiceLifetime?
        services.AddTransient<TService>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static void AddTimeProvider(this IServiceCollection services)
    {
        services.TryAddSingleton((_) => TimeProvider.System);
    }

    public static void AddRandom(this IServiceCollection services)
    {
        services.TryAddSingleton((_) => Random.Shared);
    }

    public static IServiceCollection AddStdCloudLogging(this IServiceCollection services)
    {
        services.AddLogging(
            l => l.AddStdCloudLogging()
        );
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
    public static ILoggingBuilder AddStdCloudLogging(this ILoggingBuilder builder)
    {
        builder.AddJsonConsole(
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
        );
        return builder;
    }

    public static string GetRequiredConnectionString(this IServiceProvider src, string connectionStringName)
    {
        var configuration = src.GetRequiredService<IConfiguration>();
        return configuration.GetRequiredConnectionString(connectionStringName);
    }
}