using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Microsoft.AspNetCore.SignalR;

using FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;
using FEFF.Extentions.SignalR.Redis;

public static class SignalRBuilderExtention
{
    /// <summary>
    /// Adds SignalRedisConnectionFactoryProxy as a source for:<br/>
    /// 1. SignalR redis connection<br/>
    /// 2. Healthcheck of this redis connection
    /// </summary>
    public static ISignalRServerBuilder AddRedisWithHealthCheckProxy(this ISignalRServerBuilder builder, Action<IRedisConfigurationFactoryBuilder> config)
    {
        // configure a redis connection
        builder.Services.AddRedisProviderOptions<SignalRedisProviderProxy>(config);

        // this Singleton proxy stores a connection (last and single) to perform a HealthCheck
        builder.Services.TryAddSingleton<SignalRedisProviderProxy>();

        // add standard 'redis-module-for-SignalR'
        builder.AddStackExchangeRedis();

        // configure 'redis-module-for-SignalR' with connection from 'SignalRedisConnectionFactoryProxy'
        builder.Services
            .AddOptions<RedisOptions>()
            .Configure<SignalRedisProviderProxy>((opts, rfc) =>
                opts.ConnectionFactory = rfc.ConnectAsync
            );

        return builder;
    }

    private static Task<IConnectionMultiplexer> ConnectAsync(this SignalRedisProviderProxy src, TextWriter? log)
    {
        return src.ConnectAsync(log : log);
    }

    /// <summary>
    /// Add Redis using configured 'ConfigurationOptions' onbject
    /// </summary>
    // public static ISignalRServerBuilder AddRedisByOptions(this ISignalRServerBuilder builder)
    // {
    //     builder.AddStackExchangeRedis();

    //     builder.Services
    //         .AddOptions<RedisOptions>()
    //         .Configure<ConfigurationOptions>((dst, srcOpts) =>
    //         {
    //             dst.ConnectionFactory = async (log) => await ConnectionMultiplexer.ConnectAsync(srcOpts, log);
    //         });

    //     return builder;
    // }

    public static IHealthChecksBuilder AddRedisConnectionForSignalRCheck(
        this IHealthChecksBuilder builder,
        string name = "RedisConnection_For_SignalR",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        return builder.AddCheck<RedisObservedConnectionHealthCheck<SignalRedisProviderProxy>>(name, null, tags, timeout);
    }
}