using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FEFF.Extentions.Testing;

using FEFF.Extentions.Redis;

/// <summary>
/// Adds prefix to redis DB & Channel.
/// </summary>
public class RedisPrefixFixture
{
    public RedisPrefixFixture(ITestApplicationBuilder appBuilder, string prefix)
    {
        appBuilder.ConfigureServices( services =>
            services.Configure<ConfigurationOptions>(x =>
            {
                x.ChannelPrefix = RedisChannel.Literal(prefix);
            })
        );
        
        // appBuilder.ConfigureServices( services =>
        //     services.Configure<RedisDatabaseFactory.Options>(x =>
        //     {
        //         x.KeyPrefix = prefix;
        //     })
        // );
    }
}