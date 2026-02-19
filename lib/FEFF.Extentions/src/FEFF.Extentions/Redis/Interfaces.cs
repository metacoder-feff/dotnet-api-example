using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public interface IRedisConfigurationBuilder
{
    OptionsBuilder<RedisConfigurationOptions> OptionsBuilder { get; }
}

public interface IRedisConfigurationFactoryBuilder : IRedisConfigurationBuilder
{
}

public interface IRedisProviderOptions
{
    ConfigurationOptions ConfigurationOptions { get; }
}