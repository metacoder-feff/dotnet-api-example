using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FEFF.Extentions.Testing;

using FEFF.Extentions.Redis;

//TODO: DRY
/// <summary>
/// Adds ChannelPrefix to redis configuration.
/// </summary>
public class RedisChannelPrefixFixture<TRedis>
where TRedis : RedisProviderBase
{
    public RedisChannelPrefixFixture(ITestApplicationBuilder appBuilder, string prefix)
    {
        appBuilder.ConfigureServices( services =>
        {
            services.AddRedisProviderOptions<TRedis>(b =>
            {
                b.Configure(x => 
                    x.ChannelPrefix = RedisChannel.Literal(prefix)
                );
            });
        });
    }
}

//TODO: DRY subclass?
/// <summary>
/// Adds KeyPrefix and ChannelPrefix to redis configuration.
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