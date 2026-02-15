using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace FEFF.Extentions.Redis;

public sealed partial class RedisConnectionManager : IAsyncDisposable
{
    private readonly SemaphoreLock _asyncLock = new();
//TODO: freeze
    private readonly Options _options;

    // Automatically reconnects
    private volatile ConnectionMultiplexer? _connection;
    public string? KeyPrefix => _options.KeyPrefix;

    public RedisConnectionManager(IOptions<Options> o)
    {
        _options = o.Value;
    }
    
    public async ValueTask DisposeAsync()
    {
        _asyncLock.Dispose();

        if (_connection != null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
    }

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

    public async Task<ConnectionMultiplexer> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        // double-check (optimization)
        if (_connection != null)
            return _connection;

        using (var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false))
        {
            if (_connection != null)
                return _connection;

            _connection = await ConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        return _connection;
    }

    private async Task<ConnectionMultiplexer> ConnectAsync(CancellationToken cancellationToken)
    {
//TODO: cancellation
        return await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions).ConfigureAwait(false);
    }
}

