using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public class RedisConnectionManager : RedisProviderBase, IRedisConnectionProvider, IAsyncDisposable
{
    private readonly SemaphoreLock _asyncLock = new();

    // Automatically reconnects
    private volatile IConnectionMultiplexer? _connection;

    // for subclasses
    public RedisConnectionManager(IRedisProviderOptions options) :base(options)
    {
    }

    // for registring this class in DI
    public RedisConnectionManager(RedisProviderOptions<RedisConnectionManager> options) :base(options)
    {
    }

    public async ValueTask DisposeAsync()
    {
        // Perform async cleanup.
        await DisposeAsyncCore().ConfigureAwait(false);

        // Dispose of unmanaged resources.
        //Dispose(false);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        _asyncLock.Dispose();

        if (_connection != null)
        {
            // send some finishing messages
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
    }

    public async Task<IConnectionMultiplexer> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        // double-check (optimization)
        if (_connection != null)
            return _connection;

        using (var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false))
        {
            // double-check (guard)
            if (_connection != null)
                return _connection;

            _connection = await ConnectAsync(cancellationToken:cancellationToken).ConfigureAwait(false);
        }

        return _connection;
    }
}