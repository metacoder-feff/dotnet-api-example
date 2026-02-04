using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;
/// <summary>
/// Redis connection factory to be used via DI container as a 'Singletone'.
/// Use single connection for multiplexing.
/// It offers high efficiency but works transparently without requiring any special code to enable it in your app.
/// </summary>
/// <remarks>
/// The main disadvantage of multiplexing compared to connection pooling is that it can't support the blocking "pop" commands (such as BLPOP) since these would stall the connection for all callers.
/// <seealso href="https://redis.io/docs/latest/develop/clients/pools-and-muxing/"/>
// </remarks>
public sealed partial class RedisConnectionFactory : IAsyncDisposable
{
//TODO: interface
    private readonly SemaphoreLock _asyncLock = new();
    private readonly ConfigurationOptions _options;

    // Automatically reconnects
    private volatile ConnectionMultiplexer? _connection;

    public RedisConnectionFactory(IOptions<ConfigurationOptions> o)
    {
        // freeze
        _options = o.Value.Clone();
        //_options = o.Value;
    }
    
    public async ValueTask DisposeAsync()
    {
        _asyncLock.Dispose();

        if (_connection != null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            //_connection = null;
        }
    }

    public async Task<ConnectionMultiplexer> GetConnectionAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        var r = await GetConnectionLockedAsync(log, cancellationToken).ConfigureAwait(false);
        //r.CheckConnection();
        return r;
    }

    private async Task<ConnectionMultiplexer> GetConnectionLockedAsync(TextWriter? log, CancellationToken cancellationToken)
    {
        if (_connection != null)
            return _connection;

        using (var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false))
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
        return await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);
    }
}