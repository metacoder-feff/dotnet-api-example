using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public interface IRedisConfigBuilder
{
    IServiceCollection Services { get; }
}

public interface IRedisConfigFactoryBuilder : IRedisConfigBuilder
{
}

public interface IRedisConfigurationBuilder
{
    IServiceCollection Services { get; }
}

public interface IRedisConfigurationFactoryBuilder
{
    IServiceCollection Services { get; }
}

// public interface IRedisConnectionFactory
// {
//     Task<IConnectionMultiplexer> CreateConnectionAsync(TextWriter? log);
// }

//TODO: IRedisDatabaseFactory
//TODO: public ConfigurationOptionsFactory.Options
//TODO: internal RedisConnectionFactory RedisDatabaseFactory ConfigurationOptionsFactory