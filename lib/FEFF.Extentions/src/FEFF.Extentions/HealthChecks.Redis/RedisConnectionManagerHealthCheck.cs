// some ideas from: 
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.Redis/RedisHealthCheck.cs

using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

internal class RedisConnectionManagerHealthCheck<T> : IHealthCheck
where T: RedisConnectionManager
{
    private T _redis;

    public RedisConnectionManagerHealthCheck(T m)
    {
        _redis = m;
    }
    
//TODO: DRY
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            var conn = await _redis.GetConnectionAsync(cancellationToken);
//TODO: timeouts        
                var error = await CheckHealthAsync(conn, cancellationToken);
                if (error != null)
                    return new HealthCheckResult(context.Registration.FailureStatus, "Redis HealthCheck failure: " + error);

                return HealthCheckResult.Healthy("Redis Connection is alive.");
        }
        catch (Exception ex)
        {
                return new HealthCheckResult(context.Registration.FailureStatus, "Redis HealthCheck exception.", ex);
        }
    }

    private static async Task<string?> CheckHealthAsync(IConnectionMultiplexer connection, CancellationToken cancellationToken)
    {
        connection.CheckConnection();

//TODO: cancellationToken
        await connection.GetDatabase().PingAsync().ConfigureAwait(false);

        // check we have access to a Single Standalone Master 
        var servers = connection.GetServers();
        var s = servers.SingleOrDefault(x => x.IsConnected && x.IsReplica == false && x.ServerType == ServerType.Standalone);
        if (s == null)
            return "Single Standalone Master not found";

//TODO: cancellationToken
        await s.PingAsync().ConfigureAwait(false);
// TODO: test can write

// TODO: add another check for replica





        //configuredOnly: true - is an error
        // because it is no matter how the cluster is working now

        // var eps = connection.GetEndPoints(configuredOnly: true);
        // if (eps.Length <= 0)
        //             return $"No endpoint found.";

        // foreach (var endPoint in eps)
        // {
        //     var server = connection.GetServer(endPoint);


        //     if (server.ServerType != ServerType.Cluster)
        //     {
        //         await connection.GetDatabase().PingAsync().ConfigureAwait(false);
        //         await server.PingAsync().ConfigureAwait(false);
        //     }
        //     else
        //     {
        //         var clusterInfo = await server.ExecuteAsync("CLUSTER", "INFO").ConfigureAwait(false);

        //         if (clusterInfo == null || clusterInfo.IsNull)
        //             return $"'CLUSTER INFO' is null or can't be read for endpoint: '{endPoint}'";

        //         if (clusterInfo.ToString().Contains("cluster_state:ok") == false)
        //             return $"'CLUSTER INFO/cluster_state' is not 'OK' for endpoint: '{endPoint}'";
        //     }
        // }
        return null;
    }
}