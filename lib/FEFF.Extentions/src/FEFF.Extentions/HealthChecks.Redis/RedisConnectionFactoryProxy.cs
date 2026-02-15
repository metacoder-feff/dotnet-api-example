using StackExchange.Redis;

namespace FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

/// <summary>
/// To use with external redis connection managers like 'SignalR' or 'Distributed cache.'<br/>
/// Main features:<br/>
/// 1. Create a connection for a consumer (e.g. SignalR)<br/>
/// 2. Provide last connection object for HealthCheck
/// </summary>
/// <remarks>
/// 1. To use as a proxy for a HealthCheck it should be registered as a Singleton.<br/>
/// 2. This class does not dispose a connection - it is responsibility of a consumer (e.g. SignalR).<br/>
/// </remarks>
public class RedisConnectionFactoryProxy : IRedisConnectionFactory, IRedisHealthConnectionProvider
{
//TODO: return stored _connection?
//TODO: store last exception?
    private readonly IRedisConnectionFactory _factory;

    private volatile IConnectionMultiplexer? _lastConnection;
    private volatile bool _isRequested;

    public bool IsConnectionRequested => _isRequested;
    public IConnectionMultiplexer? ActiveConnection => _lastConnection;

    public RedisConnectionFactoryProxy(IRedisConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IConnectionMultiplexer> ConnectAsync(TextWriter? log)
    {
        _isRequested = true;

//TODO (StackExchange.Redis): cancellationToken
        var res = await _factory.ConnectAsync(log).ConfigureAwait(false);

        _lastConnection = res;

        return res;
    }
}