using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.Redis;
using Microsoft.Extensions.Options;

public static class DependencyInjectionExtensions
{
    // split interfaces for different use-cases
    // implement single realization for simplicity
    internal record Builder(OptionsBuilder<RedisConnectionFactory.Options> OptionsBuilder) : IRedisConfigurationFactoryBuilder, IRedisConfigurationBuilder;
    
    /// <summary>
    /// Register transient <see cref="RedisConnectionFactory&lt;TDiscriminator&gt;"/> with its configuration.
    /// </summary>
    /// <typeparam name="TDiscriminator">A type used to distinguish different configs/factories - typically a type of consumer of <see cref="RedisConnectionFactory"/></typeparam>
    /// <param name="config">The redis connection configuration delegate.</param>
    /// <remarks>
    /// RedisConnectionFactories with different 'TDiscriminator' have different configurations.
    /// </remarks>
    public static IServiceCollection AddRedisConnectionFactory<TDiscriminator>(this IServiceCollection services, Action<IRedisConfigurationFactoryBuilder> config)
    where TDiscriminator : class
    {
// TODO: test different options created and used for different factories
        services.TryAddTransient<RedisConnectionFactory<TDiscriminator>>();

        var name = TypeHelper.GetTypeName<TDiscriminator>();
        var optsBuilder = services.AddOptions<RedisConnectionFactory.Options>(name);
        var builder = new Builder(optsBuilder);
        config(builder);

        return services;
    }

    public static IServiceCollection AddRedis<T>(this IServiceCollection services, Action<IRedisConfigurationFactoryBuilder> config)
    where T: RedisConnectionManager
    {
        services.TryAddSingleton<T>();
        services.AddRedisConnectionFactory<T>(config);
        return services;
    }

    /// <summary>
    /// Use this method to override setting parsed from ConnectionString and to setup additional settings.
    /// </summary>
    public static IRedisConfigurationBuilder Configure(this IRedisConfigurationBuilder builder,  Action<ConfigurationOptions> config)
    {
        builder.OptionsBuilder
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
    public static IRedisConfigurationBuilder ReadConnectionString(this IRedisConfigurationFactoryBuilder builder, string connectionStringName, bool ignoreUnknown = false)
    {
        builder.OptionsBuilder
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
    public static IRedisConfigurationBuilder ParseConfiguration(this IRedisConfigurationFactoryBuilder builder, string configuration, bool ignoreUnknown = false)
    {
        builder.OptionsBuilder
            .Configure( (opts) =>
                opts.ConfigurationOptions = ConfigurationOptions.Parse(configuration, ignoreUnknown)
            );
            
        return builder;
    }
    public static IRedisConfigurationBuilder SetLoggerFactory(this IRedisConfigurationBuilder builder)
    {
        builder.OptionsBuilder
            .Configure<ILoggerFactory>(
                (opt, factory) => opt.ConfigurationOptions.LoggerFactory = factory
            );

        return builder;
    }
}