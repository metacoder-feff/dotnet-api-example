using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.Redis;

public static class DependencyInjectionExtensions
{
    // split interfaces for different use-cases
    // implement single realization for simplicity
    internal record Builder(IServiceCollection Services) : IRedisConfigurationBuilder, IRedisConfigurationFactoryBuilder, IRedisConfigFactoryBuilder;

    /// <summary>
    /// Add 'RedisConnectionFactory' with no configuration.<br/>
    /// Configuration may be overriden in chained calls.
    /// </summary>
    public static IRedisConfigurationBuilder AddRedisConfiguration(this IServiceCollection services)
    {
        // by default configuration subsysten creates TOption via new()
        // use IOptionsFactory to create 'ConfigurationOptions' via 'ConfigurationOptions.Parse(..)'
        services.TryAddTransient<
            IOptionsFactory<ConfigurationOptions>, 
            ConfigurationOptionsFactory
            >();

        return new Builder(services);
    }

    /// <summary>
    /// Add 'RedisConnectionFactory' with configuration parsed from ConnectionString.<br/>
    /// Configuration may be overriden in chained calls.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionStringName">The NAME of the connection string in the configuration section.</param>
    /// <param name="ignoreUnknown">see: 'Redis.ConfigurationOptions.Parse(...)'</param>
    /// <returns></returns>
    public static IRedisConfigurationBuilder AddRedisConfiguration(this IServiceCollection services, string connectionStringName, bool ignoreUnknown = false)
    {
        return services.AddRedisConfiguration( o => 
            o.UseConnectionStringByName(connectionStringName, ignoreUnknown)
        );
    }

    /// <summary>
    /// Add 'RedisConnectionFactory' with customized factory for 'ConfigurationOptions'.<br/>
    /// Configuration may be overriden in chained calls.
    /// </summary>
    public static IRedisConfigurationBuilder AddRedisConfiguration(this IServiceCollection services, Action<IRedisConfigurationFactoryBuilder> factoryConfig)
    {
        // use 'IConfigurationFactoryBuilder' to GetConnectionString by name 
        var builder = new Builder(services);
        factoryConfig(builder);
        return services.AddRedisConfiguration();
    }

    /// <summary>
    /// Find and parse connection string to create 'ConfigurationOptions'.
    /// </summary>
    /// <param name="connectionStringName">The NAME of the connection string in the configuration section.</param>
    /// <param name="ignoreUnknown">see: 'Redis.ConfigurationOptions.Parse(...)'</param>
    public static void UseConnectionStringByName(this IRedisConfigurationFactoryBuilder builder, string connectionStringName, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<ConfigurationOptionsFactory.Options>()
            .Configure<IConfiguration>((opts, conf) =>
            {
                var cs = conf.GetRequiredConnectionString(connectionStringName);
                opts.SetParseFactoryWith(cs, ignoreUnknown);
            });
    }

    /// <summary>
    /// Parse a given 'configuration' string to create 'ConfigurationOptions'. <br/>
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
    /// <param name="ignoreUnknown">see: 'Redis.ConfigurationOptions.Parse(...)'</param>
    public static void UseConfigurationFromString(this IRedisConfigurationFactoryBuilder builder, string configuration, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<ConfigurationOptionsFactory.Options>()
            .Configure((opts) => 
                opts.SetParseFactoryWith(configuration, ignoreUnknown)
            );
    }

    public static IRedisConfigurationBuilder AddLoggerFactory(this IRedisConfigurationBuilder src)
    {
        src.Services.AddOptions<ConfigurationOptions>()
        .Configure<ILoggerFactory>(
            (opt, factory) => opt.LoggerFactory = factory
        );

        return src;
    }

    internal static void SetParseFactoryWith(this ConfigurationOptionsFactory.Options src, string configuration, bool ignoreUnknown)
    {
        src.Factory = () => ConfigurationOptions.Parse(configuration, ignoreUnknown);
    }

    /// <summary>
    /// Use this method to override setting parsed from ConnectionString and to setup additional settings.
    /// </summary>
    public static IRedisConfigurationBuilder Configure(this IRedisConfigurationBuilder builder,  Action<ConfigurationOptions> config)
    {
        builder.Services.AddOptions<ConfigurationOptions>()
            .Configure(config);
            
        return builder;
    }
    
    // public static IServiceCollection AddRedisDatabaseFactory(this IServiceCollection services, Action<RedisDatabaseFactory.Options>? config = null)
    // {
    //     services.AddRedisConnectionFactory();
    //     services.TryAddTransient<RedisDatabaseFactory>();
    //     if(config != null)
    //     {
    //         services
    //             .AddOptions<RedisDatabaseFactory.Options>()
    //             .Configure(config);
    //     }
    //     return services;
    // }

    
    public static IServiceCollection AddRedisConnectioManager(this IServiceCollection services, Action<IRedisConfigFactoryBuilder> config)
    {
        services.TryAddTransient<RedisConnectionFactory>();
        services.TryAddSingleton<RedisConnectionManager>();
        var builder = new Builder(services);
        config(builder);
        return services;
    }

    public static IRedisConfigBuilder Configure(this IRedisConfigBuilder builder,  Action<ConfigurationOptions> config)
    {
        builder.Services.AddOptions<RedisConnectionFactory.Options>()
            .Configure(x => config(x.ConfigurationOptions));
            
        return builder;
    }

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