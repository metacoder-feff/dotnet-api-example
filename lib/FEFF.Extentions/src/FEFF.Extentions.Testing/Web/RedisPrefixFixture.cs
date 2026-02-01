using Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.Redis;
using StackExchange.Redis;

namespace FEFF.Extentions.Testing;

/// <summary>
/// Adds prefix to redis DB & Channel.
/// </summary>
public class RedisPrefixFixture
{
    public RedisPrefixFixture(ITestApplicationBuilder appBuilder, string prefix)
    {
        appBuilder.ConfigureServices( services =>
            services.Configure<RedisConnectionFactory.Options>(x =>
            {
                x.KeyPrefix = prefix;
                x.ConfigurationOptions.ChannelPrefix = RedisChannel.Literal(prefix);
            })
        );
    }
}