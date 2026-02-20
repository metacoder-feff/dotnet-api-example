using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FEFF.Extentions.Testing;

using FEFF.Extentions.Redis;

/// <summary>
/// Adds prefix to redis DB & Channel.
/// </summary>
public class RedisPrefixFixture<TRedis>
where TRedis : RedisConnectionManager
{
    public RedisPrefixFixture(ITestApplicationBuilder appBuilder, string prefix)
    {
        appBuilder.ConfigureServices( services =>
        {
            services.AddRedisProviderOptions<TRedis>(b =>
            {
                b.Configure(x => 
                    x.ChannelPrefix = RedisChannel.Literal(prefix)
                );
            });

            services.AddOptions<RedisDatabaseProvider<TRedis>.Options>()
                .Configure(x => x.KeyPrefix = prefix);
        });
    }
}