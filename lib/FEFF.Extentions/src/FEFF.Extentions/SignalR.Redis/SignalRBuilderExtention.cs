using StackExchange.Redis;

namespace Microsoft.AspNetCore.SignalR.StackExchangeRedis;

public static class SignalRBuilderExtention
{
    public static ISignalRServerBuilder AddRedisConnectionFactory(this ISignalRServerBuilder builder)
    {        
        builder.AddStackExchangeRedis();
        builder.Services
            .AddOptions<RedisOptions>()
            .Configure<RedisConnectionFactory>((opts, rfc) =>
                opts.ConnectionFactory = rfc.GetConnectionAsync
            );

        return builder;
    }

    internal static async Task<IConnectionMultiplexer> GetConnectionAsync(this RedisConnectionFactory src, TextWriter log)
    {
        return await src.GetConnectionAsync(log).ConfigureAwait(false);
    }
}