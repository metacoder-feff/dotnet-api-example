using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FEFF.Extentions.Testing;
using FEFF.Extentions.Redis;

public class TestIdFixture
{
    public string TestId {get;} = Guid.NewGuid().ToString();
}

//TODO: DRY
/// <summary>
/// Adds ChannelPrefix to redis configuration.
/// </summary>
public class RedisChannelPrefixFixture<TRedis>
where TRedis : RedisProviderBase
{
    public RedisChannelPrefixFixture(ITestApplicationBuilder appBuilder, TestIdFixture testId)
    {
        appBuilder.ConfigureServices( services =>
        {
            services.AddRedisProviderOptions<TRedis>(b =>
            {
                b.Configure(x => 
                    x.ChannelPrefix = RedisChannel.Literal($"test-{testId.TestId}-")
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
    public RedisPrefixFixture(ITestApplicationBuilder appBuilder, TestIdFixture testId)
    {
        appBuilder.ConfigureServices( services =>
        {
            services.AddRedisProviderOptions<TRedis>(b =>
            {
                b.Configure(x => 
                    x.ChannelPrefix = RedisChannel.Literal($"test-{testId.TestId}-")
                );
            });

            services.AddOptions<RedisDatabaseProvider<TRedis>.Options>()
                .Configure(x => x.KeyPrefix = $"test-{testId.TestId}-");
        });
    }
}