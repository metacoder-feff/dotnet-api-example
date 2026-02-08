using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using StackExchange.Redis;

namespace Microsoft.AspNetCore.SignalR;

using FEFF.Extentions.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class SignalRBuilderExtention
{
    public static ISignalRServerBuilder UseRedisConnectionProxy(this ISignalRServerBuilder builder)
    {        
        builder.AddStackExchangeRedis();
        builder.Services.TryAddSingleton<RedisConnectionProxy>();
        builder.Services
            .AddOptions<RedisOptions>()
            .Configure<RedisConnectionProxy>((opts, rfc) =>
                opts.ConnectionFactory = rfc.GetConnectionAsync
            );

        return builder;
    }

    private static async Task<IConnectionMultiplexer> GetConnectionAsync(this RedisConnectionProxy src, TextWriter log)
    {
        return await src.GetConnectionAsync(log).ConfigureAwait(false);
    }
}