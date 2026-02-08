using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.HealthChecks.Redis;

//TODO: docs
public class RedisConnectionFactoryProxy
{
//TODO: return stored _connection?
//TODO: cancellationToken
//TODO: store last exception?

    //private readonly SemaphoreLock _asyncLock = new();

    private readonly ConfigurationOptions _options;

    private volatile ConnectionMultiplexer? _lastConnection;
    private volatile bool _isRequested;

    public bool IsRequested => _isRequested;
    public ConnectionMultiplexer? Connection => _lastConnection;

    public RedisConnectionFactoryProxy(IOptions<ConfigurationOptions> o)
    {
        // freeze
        _options = o.Value.Clone();
    }


//TODO: cancellationToken
    public async Task<IConnectionMultiplexer> CreateConnectionAsync(TextWriter? log)
    {
        _isRequested = true;

        //using var l = await _asyncLock.EnterAsync(cancellationToken).ConfigureAwait(false);

//TODO: cancellationToken
        var res = await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);

        _lastConnection = res;

        return res;
    }
}