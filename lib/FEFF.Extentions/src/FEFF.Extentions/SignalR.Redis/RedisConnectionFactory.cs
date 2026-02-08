using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.SignalR.Redis;

// public interface IRedisConnectionFactory
// {
//     Task<IConnectionMultiplexer> CreateConnectionAsync(TextWriter? log);
// }

//TODO: docs
public class RedisConnectionFactory //: IRedisConnectionFactory
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