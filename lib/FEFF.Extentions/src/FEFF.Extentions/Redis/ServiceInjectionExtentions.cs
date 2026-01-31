using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FEFF.Extentions.Redis;

//TODO: DOCS
public static class ServiceInjectionExtentions
{
    internal record RedisConnectrionManagerBuilder(IServiceCollection Services) : IRedisConnectrionManagerBuilder;

    public static IRedisConnectrionManagerBuilder AddRedisConnectrionManager(this IServiceCollection services)
    {
        services.TryAddSingleton<RedisConnectrionManager>();
        return new RedisConnectrionManagerBuilder(services);
    }

    public static IRedisConnectrionManagerBuilder UseConnectionStringByName(this IRedisConnectrionManagerBuilder builder, string connectionStringName, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<RedisConnectrionManager.Options>()
            .Configure<IConfiguration>((opts, conf) =>
            {
                var cs = conf.GetConnectionString(connectionStringName)
                            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

                opts.ConfigurationOptions.ApplyConfigurationString(cs, ignoreUnknown);
                
            });
        return builder;
    }

    public static IRedisConnectrionManagerBuilder UseConfigurationFromString(this IRedisConnectrionManagerBuilder builder, string configuration, bool ignoreUnknown = false)
    {
        builder.Services.AddOptions<RedisConnectrionManager.Options>()
            .Configure((opts) => 
                opts.ConfigurationOptions.ApplyConfigurationString(configuration, ignoreUnknown)
            );
        return builder;
    }

    public static IRedisConnectrionManagerBuilder Configure(this IRedisConnectrionManagerBuilder builder,  Action<RedisConnectrionManager.Options> config)
    {
        builder.Services.AddOptions<RedisConnectrionManager.Options>()
            .Configure(config);
            
        return builder;
    }
}

public interface IRedisConnectrionManagerBuilder
{
    IServiceCollection Services { get; }
}