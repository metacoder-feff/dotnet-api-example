using System.Collections;
using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection.Extensions;

//TODO: split namespaces
namespace FEFF.Extentions;

public static class ServiceCollectionExtention
{
    public static void AddTimeProvider(this IServiceCollection services)
    {
        services.TryAddSingleton((_) => TimeProvider.System);
    }

    public static void AddRandom(this IServiceCollection services)
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

        ThrowHelper.Assert(oldD.Lifetime == ServiceLifetime.Singleton);

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

    //TODO: split class IServiceProviderExtention
    //TODO: refactor??
    public static string GetRequiredConnectionString(this IServiceProvider src, string connectionStringName)
    {
        var configuration = src.GetRequiredService<IConfiguration>();
        return configuration.GetRequiredConnectionString(connectionStringName);
    }

    public static string GetRequiredConnectionString(this IConfiguration configuration, string connectionStringName)
    {
        var r = configuration.GetConnectionString(connectionStringName);

        if (r == null)
            throw new InvalidOperationException($"ConnectionString not found: '{connectionStringName}'");

        return r;
    }

    public static bool GetReloadConfigOnChangeValue(this IConfiguration configuration)
    {
        const string reloadConfigOnChangeKey = "hostBuilder:reloadConfigOnChange";
        var result = true;
        if (configuration[reloadConfigOnChangeKey] is string reloadConfigOnChange)
        {
            if (!bool.TryParse(reloadConfigOnChange, out result))
            {
                throw new InvalidOperationException($"Failed to convert configuration value at '{configuration.GetSection(reloadConfigOnChangeKey).Path}' to type '{typeof(bool)}'.");
            }
        }
        return result;
    }
}

public static class EnvironmentHelper
{
    /// <summary>
    /// On linux FileWatcher spends ulimit very fast.
    /// The exception generated:
    ///   "The configured user limit (128) on the number of inotify..."
    /// Otherwise you need extend ulimit at the host (container).
    /// The setting 'reloadConfigOnChange' is treated as enabled by default.
    /// This method will set this setting to false only if it is not defined yet.
    /// Env setting used to re-enable is: "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=true".
    /// </summary>
    public static void DisableReloadConfigByDefault()
    {
        // WebApplication.CreateBuilder uses constructor new WebApplicationBuilder(...)
        // which uses ConfigurationManager values to add config files to ConfigurationManager
        // At that moment ConfigurationManager is set only from Environment 
        // by: EnvironmentVariablesExtensions.AddEnvironmentVariables(this ..., string? prefix)

        var reloadConfigOnChangeKey = "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE";
        if(Environment.GetEnvironmentVariable(reloadConfigOnChangeKey) == null)
        {
            Environment.SetEnvironmentVariable(reloadConfigOnChangeKey, "false");
        }
    }

    /// <summary>
    /// Typed version of Environment.GetEnvironmentVariables().
    /// Skips env records when key or value is null, that typically should not occur.
    /// </summary>
    public static FrozenDictionary<string, string> GetEnvironmentVariables()
    {
        return Environment
                    .GetEnvironmentVariables()
                    .Cast<DictionaryEntry>()
                    .Select(TryMakeTyped<string, string>)
                    .WhereNotNull()
                    .ToFrozenDictionary();
    }

    private static KeyValuePair<TKey, TVal>? TryMakeTyped<TKey, TVal>(DictionaryEntry src)
    {
        if(src.Key is TKey k == false)
            return null;

        if(src.Value is TVal v == false)
            return null;

        return new KeyValuePair<TKey, TVal>(k, v);
    }

//TODO: test
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
    where T : struct
    {
        // return enumerable.Where(e => e != null).Select(e => e!);
        foreach (var item in sequence)
        {
            if (item == null)
                continue;
            yield return item.Value;
        }
    }

//TODO: test
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
    where T : notnull
    {
        // return enumerable.Where(e => e != null).Select(e => e!);
        foreach (var item in sequence)
        {
            if (item == null)
                continue;
            yield return item;
        }
    }
    
//TODO: test
    // returns 'null' only when not found. Only allowed when 'TVal : notnull'.
    public static TVal? TryGetOrNull<TKey, TVal>(this IDictionary<TKey, TVal>src, TKey key)
    where TVal : notnull
    {
        var b = src.TryGetValue(key, out var value);
        if (b == false)
            return default;
        return value;
    }

//TODO: test
    // returns 'null' whether not found or found null
    public static TVal? TryGetBothNull<TKey, TVal>(this IDictionary<TKey, TVal?>src, TKey key)
    where TVal : notnull
    {
        var b = src.TryGetValue(key, out var value);
        if (b == false)
            return default;
        return value;
    }
}