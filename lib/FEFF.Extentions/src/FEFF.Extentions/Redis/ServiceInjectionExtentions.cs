using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public static class ServiceInjectionExtentions
{
    // split interfaces for different use-cases
    // implement single realization for simplicity
    internal record Builder(IServiceCollection Services) : IRedisConnectrionManagerBuilder, IConfigurationFactoryBuilder;

    /// <summary>
    /// Add 'RedisConnectrionManager' with no configuration.<br/>
    /// Configuration may be overriden in chained calls.
    /// </summary>
    public static IRedisConnectrionManagerBuilder AddRedisConnectrionManager(this IServiceCollection services)
    {
        // by default configuration subsysten creates TOption via new()
        // use IOptionsFactory to create 'ConfigurationOptions' via 'ConfigurationOptions.Parse(..)'
        services.TryAddTransient<
            IOptionsFactory<RedisConnectionFactory.Options>, 
            RedisConnectrionManagerOptionsFactory
            >();

        services.TryAddSingleton<RedisConnectionFactory>();
        return new Builder(services);
    }

    /// <summary>
    /// Add 'RedisConnectrionManager' with configuration parsed from ConnectionString.<br/>
    /// Configuration may be overriden in chained calls.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionStringName">The NAME of the connection string in the configuration section.</param>
    /// <param name="ignoreUnknown">see: 'Redis.ConfigurationOptions.Parse(...)'</param>
    /// <returns></returns>
    public static IRedisConnectrionManagerBuilder AddRedisConnectrionManager(this IServiceCollection services, string connectionStringName, bool ignoreUnknown = false)
    {
        return services.AddRedisConnectrionManager( o => 
            o.UseConnectionStringByName(connectionStringName, ignoreUnknown)
        );
    }

    /// <summary>
    /// Add 'RedisConnectrionManager' with customized factory for 'ConfigurationOptions'.<br/>
    /// Configuration may be overriden in chained calls.
    /// </summary>
    public static IRedisConnectrionManagerBuilder AddRedisConnectrionManager(this IServiceCollection services, Action<IConfigurationFactoryBuilder> factoryConfig)
    {
        // use 'IConfigurationFactoryBuilder' to GetConnectionString by name 
        var builder = new Builder(services);
        factoryConfig(builder);
        return services.AddRedisConnectrionManager();
    }

    /// <summary>
    /// Find and parse connection string to create 'ConfigurationOptions'.
    /// </summary>
    /// <param name="connectionStringName">The NAME of the connection string in the configuration section.</param>
    /// <param name="ignoreUnknown">see: 'Redis.ConfigurationOptions.Parse(...)'</param>
    public static void UseConnectionStringByName(this IConfigurationFactoryBuilder builder, string connectionStringName, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<RedisConnectrionManagerOptionsFactory.Options>()
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
    public static void UseConfigurationFromString(this IConfigurationFactoryBuilder builder, string configuration, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<RedisConnectrionManagerOptionsFactory.Options>()
            .Configure((opts) => 
                opts.SetParseFactoryWith(configuration, ignoreUnknown)
            );
    }

    internal static void SetParseFactoryWith(this RedisConnectrionManagerOptionsFactory.Options src, string configuration, bool ignoreUnknown)
    {
        src.Factory = () => ConfigurationOptions.Parse(configuration, ignoreUnknown);
    }

    /// <summary>
    /// Use this method to override setting parsed from ConnectionString and to setup additional settings.
    /// </summary>
    public static IRedisConnectrionManagerBuilder Configure(this IRedisConnectrionManagerBuilder builder,  Action<RedisConnectionFactory.Options> config)
    {
        builder.Services.AddOptions<RedisConnectionFactory.Options>()
            .Configure(config);
            
        return builder;
    }
}

public interface IRedisConnectrionManagerBuilder
{
    IServiceCollection Services { get; }
}

public interface IConfigurationFactoryBuilder
{
    IServiceCollection Services { get; }
}

public class RedisConnectrionManagerOptionsFactory : OptionsFactory<RedisConnectionFactory.Options>
{
    private readonly Func<ConfigurationOptions> _factory;

    public RedisConnectrionManagerOptionsFactory(
        IEnumerable<IConfigureOptions<RedisConnectionFactory.Options>> setups, 
        IEnumerable<IPostConfigureOptions<RedisConnectionFactory.Options>> postConfigures,
        IOptions<Options> factoryOpts) 
    : base(setups, postConfigures)
    {
        _factory = factoryOpts.Value.Factory;
    }

    protected override RedisConnectionFactory.Options CreateInstance(string name)
    {
        return new RedisConnectionFactory.Options
        {
            ConfigurationOptions = _factory() // e.g. ConfigurationOptions.Parse(...)
        };
        //return base.CreateInstance(name);
    }

    public class Options
    {
        public Func<ConfigurationOptions> Factory { get; set; } = DefaultFactory;
        
        public static ConfigurationOptions DefaultFactory() => new();
    }
}