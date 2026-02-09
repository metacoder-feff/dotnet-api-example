using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.HealthChecks.Redis;

/// <summary>
/// To use with external redis connection managers like 'SignalR' or 'Distributed cache.'</br>
/// Main features:</br>
/// 1. Use IOptions<ConfigurationOptions> that is defined once for many connections.</br>
/// 2. Create a connection for a consumer (e.g. SignalR)</br>
/// 2. Provide last connection object for HealthCheck
/// </summary>
/// <remarks>
/// 1. To use as a proxy for a HealthCheck it should be registered as a Singleton.</br>
/// 2. This class does not dispose a connection - it is responsibility of a consumer (SignalR).</br>
/// </remarks>
public class RedisConnectionFactoryProxy : /*IRedisConnectionFactory,*/ IRedisHealthConnectionProvider
{
//TODO: split proxy responsibility


//TODO: return stored _connection?
//TODO: cancellationToken
//TODO: store last exception?

    //private readonly SemaphoreLock _asyncLock = new();

    private readonly ConfigurationOptions _options;

    private volatile ConnectionMultiplexer? _lastConnection;
    private volatile bool _isRequested;

    public bool IsConnectionRequested => _isRequested;
    public ConnectionMultiplexer? ActiveConnection => _lastConnection;

    public RedisConnectionFactoryProxy(IOptions<ConfigurationOptions> o)
    {
        // freeze
        _options = o.Value.Clone();
    }


//TODO: cancellationToken
    public async Task<IConnectionMultiplexer> CreateConnectionAsync(TextWriter? log)
    {
        _isRequested = true;

        //using var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false);

//TODO: cancellationToken
        var res = await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);

        _lastConnection = res;

        return res;
    }
}