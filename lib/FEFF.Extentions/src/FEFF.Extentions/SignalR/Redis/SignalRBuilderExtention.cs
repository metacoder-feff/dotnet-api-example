using FEFF.Extentions.Redis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using StackExchange.Redis;

namespace FEFF.Extentions.SignalR.Redis;

public static class SignalRBuilderExtention
{
    public static ISignalRServerBuilder AddRedisConnectionManager(this ISignalRServerBuilder builder)
    {        
        builder.AddStackExchangeRedis();
        builder.Services
            .AddOptions<RedisOptions>()
            .Configure<RedisConnectionManager>((opts, redisManager) =>
                opts.ConnectionFactory = redisManager.GetConnectionAsync
            );

        return builder;
    }

    internal static async Task<IConnectionMultiplexer> GetConnectionAsync(this RedisConnectionManager src, TextWriter log)
    {
        return await src.GetConnectionAsync(log).ConfigureAwait(false);
    }
}