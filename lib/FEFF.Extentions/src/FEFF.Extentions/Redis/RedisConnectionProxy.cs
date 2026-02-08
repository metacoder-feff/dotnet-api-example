using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

//TODO: docs
public class RedisConnectionProxy
{
//TODO: distinguish multiple proxies
//TODO: return stored _connection?
//TODO: cancellationToken

    //private readonly SemaphoreLock _asyncLock = new();

    private readonly ConfigurationOptions _options;

    private volatile ConnectionMultiplexer? _lastConnection;
    private volatile bool _isStarted;

    public bool IsStarted => _isStarted;
    public ConnectionMultiplexer? Connection => _lastConnection;

    public RedisConnectionProxy(IOptions<ConfigurationOptions> o)
    {
        // freeze
        _options = o.Value.Clone();
    }

    public async Task<ConnectionMultiplexer> GetConnectionAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        _isStarted = true;

        //using var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false);

//TODO: cancellationToken
        var res = await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);

        _lastConnection = res;

        return res;
    }
}