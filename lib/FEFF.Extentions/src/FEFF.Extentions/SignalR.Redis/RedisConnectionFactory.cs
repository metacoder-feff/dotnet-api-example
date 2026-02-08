using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.SignalR.Redis;

//TODO: docs
public class RedisConnectionFactory
{
//TODO: distinguish multiple proxies/factories
//TODO: return stored _connection?
//TODO: cancellationToken
//TODO: store last exception?

    //private readonly SemaphoreLock _asyncLock = new();

    private readonly ConfigurationOptions _options;

    private volatile ConnectionMultiplexer? _lastConnection;
    private volatile bool _isRequested;

    public bool IsRequested => _isRequested;
    public ConnectionMultiplexer? Connection => _lastConnection;

    public RedisConnectionFactory(IOptions<ConfigurationOptions> o)
    {
        // freeze
        _options = o.Value.Clone();
    }

    public async Task<ConnectionMultiplexer> GetConnectionAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        _isRequested = true;

        //using var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false);

//TODO: cancellationToken
        var res = await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);

        _lastConnection = res;

        return res;
    }
}