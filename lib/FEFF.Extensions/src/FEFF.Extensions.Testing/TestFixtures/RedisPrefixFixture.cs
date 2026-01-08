using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FEFF.Experimental.TestFixtures.AspNetCore;
using FEFF.Extensions.Redis;
using FEFF.TestFixtures;
using FEFF.TestFixtures.AspNetCore;

//TODO: DRY
/// <summary>
/// Adds ChannelPrefix to redis configuration in a tested application.
/// </summary>
[Fixture]
public class RedisChannelPrefixFixture<TEntryPoint,TRedis>
where TEntryPoint: class
where TRedis : RedisProviderBase
{
    public RedisChannelPrefixFixture(AppManagerFixture<TEntryPoint> app, TmpScopeIdFixture testId)
    {
        app.ConfigurationBuilder.ConfigureServices( services =>
        {
            services.AddRedisProviderOptions<TRedis>(b =>
            {
                b.Configure(x => 
                    x.ChannelPrefix = RedisChannel.Literal($"test-{testId.Value}-")
                );
            });
        });
    }
}

//TODO: DRY subclass?
/// <summary>
/// Adds KeyPrefix and ChannelPrefix to redis configuration in a tested application.
/// </summary>
[Fixture]
public class TmpRedisPrefixFixture<TEntryPoint,TRedis>
where TEntryPoint: class
where TRedis : RedisConnectionManager
{
    public TmpRedisPrefixFixture(AppManagerFixture<TEntryPoint> app, TmpScopeIdFixture testId)
    {
        app.ConfigurationBuilder.ConfigureServices( services =>
        {
            services.AddRedisProviderOptions<TRedis>(b =>
            {
                b.Configure(x => 
                    x.ChannelPrefix = RedisChannel.Literal($"test-{testId.Value}-")
                );
            });

            services.AddOptions<RedisDatabaseProvider<TRedis>.Options>()
                .Configure(x => x.KeyPrefix = $"test-{testId.Value}-");
        });
    }
}