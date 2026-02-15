using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.Redis;

public static class DependencyInjectionExtensions
{
    // split interfaces for different use-cases
    // implement single realization for simplicity
    internal record Builder(IServiceCollection Services) : IRedisConfigurationBuilder, IRedisConfigurationFactoryBuilder, IRedisConfigFactoryBuilder;

    public static IServiceCollection AddRedisConnectionFactory(this IServiceCollection services, Action<IRedisConfigFactoryBuilder> config)
    {
        services.TryAddTransient<RedisConnectionFactory>();
        var builder = new Builder(services);
        config(builder);
        return services;
    }

    public static IServiceCollection AddRedisConnectionManager(this IServiceCollection services, Action<IRedisConfigFactoryBuilder> config)
    {
        services.TryAddSingleton<RedisConnectionManager>();
        services.AddRedisConnectionFactory(config);
        return services;
    }

    /// <summary>
    /// Use this method to override setting parsed from ConnectionString and to setup additional settings.
    /// </summary>
    public static IRedisConfigBuilder Configure(this IRedisConfigBuilder builder,  Action<ConfigurationOptions> config)
    {
        builder.Services.AddOptions<RedisConnectionFactory.Options>()
            .Configure(x => config(x.ConfigurationOptions));
            
        return builder;
    }

    /// <summary>
    /// Read connection-string form application configuration, parse it and set <see cref="ConfigurationOptions"/> for redis.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionStringName">The NAME of the connection string in the configuration section.</param>
    /// <param name="ignoreUnknown">see: <see cref="ConfigurationOptions.Parse(string, bool)"/></param>
    /// <remarks>
    /// Rewrites whole <see cref="ConfigurationOptions"/> object.
    /// </remarks>
    public static IRedisConfigBuilder ReadConnectionString(this IRedisConfigFactoryBuilder builder, string connectionStringName, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<RedisConnectionFactory.Options>()
            .Configure<IConfiguration>((opts, conf) =>
            {
                var cs = conf.GetRequiredConnectionString(connectionStringName);
                opts.ConfigurationOptions = ConfigurationOptions.Parse(cs, ignoreUnknown);
            });
            
        return builder;
    }

    /// <summary>
    /// Parse a given 'configuration' string to create <see cref="ConfigurationOptions"/>. <br/>
    /// https://redis.io/docs/latest/develop/connect/clients/dotnet/ <br/>
    /// https://stackexchange.github.io/StackExchange.Redis/
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration">
    /// ConfigurationString examples: <br/>
    /// 1. This will connect to a single server on the local machine using the default redis port (6379). 
    ///     "localhost" <br/>
    ///
    /// 2. Additional options are simply appended (comma-delimited). Ports are represented with a colon (:) as is usual.
    /// Configuration options include an = after the name. For example:
    ///     "redis0:6380,redis1:6380,allowAdmin=true" <br/>
    ///
    /// 3. If you specify a serviceName in the connection string, it will trigger sentinel mode.
    /// This example will connect to a sentinel server on the local machine using the default sentinel port (26379), 
    /// discover the current primary server for the myprimary service and return a managed connection pointing to that 
    /// primary server that will automatically be updated if the primary changes:
    ///     "localhost,serviceName=myprimary" <br/>
    ///
    ///  4.
    ///     $"{HOST_NAME}:{PORT_NUMBER},password={PASSWORD}"
    /// </param>
    /// <param name="ignoreUnknown">see: <see cref="ConfigurationOptions.Parse(string, bool)"/></param>
    /// <remarks>
    /// Rewrites whole <see cref="ConfigurationOptions"/> object.
    /// </remarks>
    public static IRedisConfigBuilder ParseConfiguration(this IRedisConfigFactoryBuilder builder, string configuration, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<RedisConnectionFactory.Options>()
            .Configure( (opts) =>
                opts.ConfigurationOptions = ConfigurationOptions.Parse(configuration, ignoreUnknown)
            );
            
        return builder;
    }
    public static IRedisConfigBuilder SetLoggerFactory(this IRedisConfigBuilder src)
    {
        src.Services.AddOptions<RedisConnectionFactory.Options>()
        .Configure<ILoggerFactory>(
            (opt, factory) => opt.ConfigurationOptions.LoggerFactory = factory
        );

        return src;
    }
}