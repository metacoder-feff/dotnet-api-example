using DotNext.Threading;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace FEFF.Extentions.Redis;

/// <summary>
/// Redis connection factory to be used via DI container as a 'Singletone'.
/// </summary>
public sealed partial class RedisConnectionFactory : IAsyncDisposable
{
    private readonly AsyncLock _asyncLock = AsyncLock.Exclusive();//  Semaphore();
//TODO: freeze options
    private readonly Options _options;

    // Automatically reconnects
    private volatile ConnectionMultiplexer? _connection;

    public RedisConnectionFactory(IOptions<Options> o)
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
    public async Task<IDatabase> GetDatabaseAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        var c = await GetConnectionAsync(log, cancellationToken).ConfigureAwait(false);
        var res = c.GetDatabase();

        var prefix = _options.KeyPrefix;
        if (prefix.IsNullOrEmpty())
            return res;

        // namespace for Redis keyspace DB API
        return res.WithKeyPrefix(prefix);
    }

    public async Task<ConnectionMultiplexer> GetConnectionAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        var r = await GetConnectionInternalAsync(log, cancellationToken).ConfigureAwait(false);
        //r.CheckConnection();
        return r;
    }

    private async Task<ConnectionMultiplexer> GetConnectionInternalAsync(TextWriter? log, CancellationToken cancellationToken)
    {

        if (_connection != null)
            return _connection;

        using (var l = await _asyncLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
        {
            if (_connection != null)
                return _connection;

            _connection = await ConnectAsync(log, cancellationToken).ConfigureAwait(false);
        }

        return _connection;
    }

    private async Task<ConnectionMultiplexer> ConnectAsync(TextWriter? log, CancellationToken cancellationToken)
    {
//TODO: cancellationToken
        return await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions, log).ConfigureAwait(false);
    }
}