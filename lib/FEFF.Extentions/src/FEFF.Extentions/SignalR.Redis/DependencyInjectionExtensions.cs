using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using StackExchange.Redis;

namespace Microsoft.AspNetCore.SignalR;

using FEFF.Extentions.Redis;

public static class SignalRBuilderExtention
{
    public static ISignalRServerBuilder UseRedisConnectionFactory(this ISignalRServerBuilder builder)
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