using StackExchange.Redis;
// using StackExchange.Redis.KeyspaceIsolation;

namespace FEFF.Extentions.Redis;

//TODO: multiple singletons with different factories/options
public sealed class RedisConnectionManager : IAsyncDisposable
{
    private readonly SemaphoreLock _asyncLock = new();

    private readonly IRedisConnectionFactory _factory;

    // Automatically reconnects
    private volatile IConnectionMultiplexer? _connection;
    // public string? KeyPrefix => _options.KeyPrefix;

    public RedisConnectionManager(RedisConnectionFactory<RedisConnectionManager> factory)
    {
        _factory = factory;
    }

    public async ValueTask DisposeAsync()
    {
        _asyncLock.Dispose();

        if (_connection != null)
        {
            // send some finishing messages
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
    }
/*/
    public async Task<IDatabase> GetDatabaseAsync(CancellationToken token = default)
    {
        // for tests
        var prefix = _options.KeyPrefix;

        // namespace for Redis pub/sub API
        // if (prefix.IsNullOrEmpty() == false)
        //     o.Configuration.ChannelPrefix = RedisChannel.Literal(prefix)

        var c = await GetConnectionAsync(token).ConfigureAwait(false);

        var res = c.GetDatabase();
        if (prefix.IsNullOrEmpty())
            return res;

        // namespace for Redis keyspace DB API
        return res.WithKeyPrefix(prefix);
    }
*/
    public async Task<IConnectionMultiplexer> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        // double-check (optimization)
        if (_connection != null)
            return _connection;

        using (var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false))
        {
            if (_connection != null)
                return _connection;

//TODO (StackExchange.Redis): cancellationToken
            _connection = await _factory.ConnectAsync().ConfigureAwait(false);
        }

        return _connection;
    }
}

