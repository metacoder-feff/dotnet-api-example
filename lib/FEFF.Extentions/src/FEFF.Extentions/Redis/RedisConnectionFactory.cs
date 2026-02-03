using DotNext.Threading;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

/// <summary>
/// Redis connection factory to be used via DI container as a 'Singletone'.
/// </summary>
//TODO: interface
public sealed partial class RedisConnectionFactory : IAsyncDisposable
{
//TODO: remove dotnext
    private readonly AsyncLock _asyncLock = AsyncLock.Exclusive();//  Semaphore();
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
        // this also waits _asyncLock to be released
        await _asyncLock.DisposeAsync().ConfigureAwait(false);

        if (_connection != null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            //_connection = null;
        }
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
        return await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);
    }
}