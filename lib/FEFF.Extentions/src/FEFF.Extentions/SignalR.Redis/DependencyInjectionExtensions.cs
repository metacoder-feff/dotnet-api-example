using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Microsoft.AspNetCore.SignalR;

using FEFF.Extentions.SignalR.Redis;

public static class SignalRBuilderExtention
{
    public static ISignalRServerBuilder UseRedisConnectionFactory(this ISignalRServerBuilder builder)
    {        
        builder.AddStackExchangeRedis();
        builder.Services.TryAddSingleton<RedisConnectionFactory>();
        builder.Services
            .AddOptions<RedisOptions>()
            .Configure<RedisConnectionFactory>((opts, rfc) =>
                opts.ConnectionFactory = rfc.GetConnectionAsync
            );

        return builder;
    }

    private static async Task<IConnectionMultiplexer> GetConnectionAsync(this RedisConnectionFactory src, TextWriter log)
    {
        return await src.GetConnectionAsync(log).ConfigureAwait(false);
    }

    public static IHealthChecksBuilder AddRedisConnectionForSignalrCheck(
        this IHealthChecksBuilder builder,
        string name = "RedisConnection_For_SignalR",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        return builder.AddCheck<RedisConnectionHealthCheck>(name, null, tags, timeout);
    }
}