using DotNext.Threading;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace FEFF.Extentions.Redis;

public sealed partial class RedisConnectrionManager : IAsyncDisposable
{
    private readonly AsyncLock _asyncLock = AsyncLock.Exclusive();//  Semaphore();
//TODO: freeze options
    private readonly Options _options;

    // Automatically reconnects
    private volatile ConnectionMultiplexer? _connection;

    public RedisConnectrionManager(IOptions<Options> o)
    {
        _options = o.Value;
    }
    
    public async ValueTask DisposeAsync()
    {
        // this also waits _asyncLock to be released
        await _asyncLock.DisposeAsync().ConfigureAwait(false);

        if (_connection != null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            //_connection = null;
//TODO: ref-counter/using
// thus for now 'RedisConnectrionManager' is singletone
        }
    }

    /// <summary>
    /// Returns a Database using 'options.KeyPrefix' (for tests).
    /// </summary>
    public async Task<IDatabase> GetDatabaseAsync(CancellationToken token = default)
    {
        var c = await GetConnectionAsync(token).ConfigureAwait(false);
        var res = c.GetDatabase();

        var prefix = _options.KeyPrefix;
        if (prefix.IsNullOrEmpty())
            return res;

        // namespace for Redis keyspace DB API
        return res.WithKeyPrefix(prefix);
    }

    public async Task<ConnectionMultiplexer> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var r = await GetConnectionInternalAsync(cancellationToken).ConfigureAwait(false);
        //r.CheckConnection();
        return r;
    }

    private async Task<ConnectionMultiplexer> GetConnectionInternalAsync(CancellationToken cancellationToken)
    {

        if (_connection != null)
            return _connection;

        using (var l = await _asyncLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
        {
            if (_connection != null)
                return _connection;

            _connection = await ConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        return _connection;
    }

    private async Task<ConnectionMultiplexer> ConnectAsync(CancellationToken cancellationToken)
    {
//TODO: cancellationToken
        return await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions).ConfigureAwait(false);
    }
}