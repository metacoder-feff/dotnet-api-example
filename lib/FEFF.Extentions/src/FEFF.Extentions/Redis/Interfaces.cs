using Microsoft.Extensions.Options;
// using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public interface IRedisConfigurationBuilder
{
    OptionsBuilder<RedisConnectionFactory.Options> OptionsBuilder { get; }
}

public interface IRedisConfigurationFactoryBuilder : IRedisConfigurationBuilder
{
}

// public interface IRedisConnectionFactory
// {
//     Task<IConnectionMultiplexer> ConnectAsync(TextWriter? log = null);
// }