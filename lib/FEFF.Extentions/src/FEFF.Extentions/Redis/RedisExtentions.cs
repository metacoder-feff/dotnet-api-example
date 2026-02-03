namespace StackExchange.Redis;

public static class RedisExtentions
{
    // Check wether redis servers are configured to be a properly cluster (master-slave server set)
    // only for Standalone:
    // - if configured multiple masters\
    // - no errors from StackExchange.Redis
    // - undefined (untested) behavior 
    public static void CheckConnection(this ConnectionMultiplexer c)
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

//TODO: add tests
    public static async Task<(long position, double score)?> SortedSetRankWithScoreAsync(this IDatabase db, RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        //https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisDatabase.cs
        //order == Order.Descending ? RedisCommand.ZREVRANK : RedisCommand.ZRANK
        var cmd = order == Order.Descending ? "ZREVRANK" : "ZRANK";
        var args = new object[] { key, member, "WITHSCORE" };

        var res = await db.ExecuteAsync(cmd, args, flags);
        if (res.IsNull)
            return null;

//TODO: Resp2?
//TODO: return null if error?
        ThrowHelper.Assert(res.Resp3Type == ResultType.Array);
        ThrowHelper.Assert(res.Length == 2);
        var r0 = res[0];        
        var r1 = res[1];
        //ThrowHelper.InvalidOperation.Assert(r0.Resp3Type == ResultType.Integer);
        //ThrowHelper.InvalidOperation.Assert(r1.Resp3Type == ResultType.BulkString);
        var p = (long)r0;
        var s = (double)r1;

        return (p, s);
    }

    //https://github.com/StackExchange/StackExchange.Redis/blob/00711481f92c06ccd4f83886e3d2b6e70718206b/src/StackExchange.Redis/ResultProcessor.cs#L585
    // public static bool TryParse(in RawResult result, out SortedSetEntry? entry)
    // {
    //     switch (result.Resp2TypeArray)
    //     {
    //         case ResultType.Array:
    //             if (result.IsNull || result.ItemsCount < 2)
    //             {
    //                 entry = null;
    //             }
    //             else
    //             {
    //                 var arr = result.GetItems();
    //                 entry = new SortedSetEntry(arr[0].AsRedisValue(), arr[1].TryGetDouble(out double val) ? val : double.NaN);
    //             }
    //             return true;
    //         default:
    //             entry = null;
    //             return false;
    //     }
    // }
}