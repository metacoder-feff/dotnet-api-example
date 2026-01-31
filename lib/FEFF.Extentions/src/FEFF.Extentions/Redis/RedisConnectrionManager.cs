using DotNext.Threading;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace FEFF.Extentions.Redis;

public sealed partial class RedisConnectrionManager : IAsyncDisposable
{
    private readonly AsyncLock _asyncLock = AsyncLock.Exclusive();//  Semaphore();
    private readonly Options _options;
    private readonly string _connStr;

    // Automatically reconnects
    private volatile ConnectionMultiplexer? _connection;

    public string? KeyPrefix => _options.KeyPrefix;

    public RedisConnectrionManager(IOptions<Options> o, IConfiguration config)
    {
        _options = o.Value;
        var n = _options.ConnectionStringName;
        _connStr = config.GetConnectionString(n)
                    ?? throw new InvalidOperationException($"Redis ConnectrionString not found: '{n}'");
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
        var r = await GetConnectionInternalAsync(cancellationToken).ConfigureAwait(false);
        CheckConnection(r);
        return r;
    }

    // Check wether redis servers are configured to be a properly cluster (master-slave server set)
    // only for Standalone:
    // - if configured multiple masters\
    // - no errors from StackExchange.Redis
    // - undefined (untested) behavior 
    private static void CheckConnection(ConnectionMultiplexer c)
    {
        var servers = c.GetServers();

        var allStandalone = servers.All(x => x.ServerType == ServerType.Standalone);
        // TOOD: better message
        if (allStandalone == false)
            throw new InvalidOperationException("Not all servers have type 'Standalone'.");

        var masterCnt = servers.Where(x => x.IsReplica == false && x.IsConnected).Count();
        if (masterCnt > 1)
            throw new InvalidOperationException("More than ONE master Standalone defined for redis connection.");
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
        var options = ConfigurationOptions.Parse(_connStr);
//TODO: additional setup
//TODO: cancellationToken
        return await ConnectionMultiplexer.ConnectAsync(options).ConfigureAwait(false);
    }
}