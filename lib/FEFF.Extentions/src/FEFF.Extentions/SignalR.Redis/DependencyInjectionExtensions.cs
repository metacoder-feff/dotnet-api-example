using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.SignalR;

using FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;
using FEFF.Extentions.SignalR.Redis;

public static class SignalRBuilderExtention
{
    public static ISignalRServerBuilder AddRedisByConnectionFactory(this ISignalRServerBuilder builder)
    {
        return builder.AddRedisByConnectionFactory<SignalRedisConnectionFactory>();
    }

    public static ISignalRServerBuilder AddRedisByConnectionFactory<TFactory>(this ISignalRServerBuilder builder)
    where TFactory : class, IRedisConnectionFactory
    {        
        builder.AddStackExchangeRedis();
        builder.Services.TryAddSingleton<TFactory>();

        builder.Services
            .AddOptions<RedisOptions>()
            .Configure<TFactory>((opts, rfc) =>
                opts.ConnectionFactory = rfc.CreateConnectionAsync
            );

        return builder;
    }

    public static IHealthChecksBuilder AddRedisConnectionForSignalRCheck(
        this IHealthChecksBuilder builder,
        string name = "RedisConnection_For_SignalR",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        return builder.AddCheck<RedisConnectionFactoryHealthCheck<SignalRedisConnectionFactory>>(name, null, tags, timeout);
    }
}