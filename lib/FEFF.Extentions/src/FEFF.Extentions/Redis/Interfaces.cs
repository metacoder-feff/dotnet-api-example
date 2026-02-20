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

// Get an established connection to a redis server/cluster.
// The connection is managed (created AND disposed) by a realization of this interface.
// 'interface of RedisConnectionManager' = 'IRedisConnectionProvider + I[Async]Disposable'.
public interface IRedisConnectionProvider
{
    Task<IConnectionMultiplexer> GetConnectionAsync(CancellationToken cancellationToken = default);
}


public interface IRedisDatabaseProvider
{
    Task<IDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default);
}