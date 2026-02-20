using StackExchange.Redis;

namespace FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

/// <summary>
/// For <see cref="RedisObservedConnectionHealthCheck&lt;&gt;"/>.<br/>
/// To use with external redis connection managers like 'SignalR' or 'Distributed cache.'<br/>
/// Main features:<br/>
/// 1. Create a connection for a consumer (e.g. SignalR)<br/>
/// 2. Provide last connection object for HealthCheck
/// </summary>
/// <remarks>
/// To use as a proxy for a HealthCheck it should be registered as a Singleton.<br/>
/// This class does not dispose a connection - it is responsibility of a consumer (e.g. SignalR).<br/>
/// </remarks>
public class RedisProviderProxy : RedisProviderBase, IRedisConnectionObservable
{
//TODO: return stored _connection?
//TODO: store last exception?

    private volatile IConnectionMultiplexer? _lastConnection;
    private volatile bool _isRequested;

    public bool IsConnectionRequested => _isRequested;
    public IConnectionMultiplexer? ActiveConnection => _lastConnection;

    public RedisProviderProxy(IRedisProviderOptions options) : base(options)
    {
    }

    public new async Task<IConnectionMultiplexer> ConnectAsync(TextWriter? log, CancellationToken cancellationToken = default)
    {
        _isRequested = true;

        var res = await base.ConnectAsync(log, cancellationToken).ConfigureAwait(false);

        _lastConnection = res;

        return res;
    }
}