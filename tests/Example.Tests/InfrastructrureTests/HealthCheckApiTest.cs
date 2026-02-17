using System.Diagnostics;

namespace Example.Tests.InfrastructrureTests;

public class HealthCheckApiTest : ApiTestBase
{
    private const string LivenesstUri = "/health/liveness";
    private const string ReadinessUri = "/health/readiness";
    private const string HealthAllUri = "/health/overview";

    [Fact]
    public async Task Liveness_should_be_ok()
    {
        var body = await GetProbeAsync(LivenesstUri);      

        body.ParseJToken()
            .ReplaceValue("duration"          , "00:00:00.555")
            .ReplaceValue("checks[*].duration", "00:00:00.555")       // remove randomness
            .Sort("checks")
            .Should().BeEquivalentTo("""
        {
            "status": "healthy",
            "duration": "00:00:00.555",
            "checks": [
                {
                "name": "SimpleHealthCheck",
                "description": "AspNet is alive.",
                "duration": "00:00:00.555",
                "status": "healthy",
                "data": {}
                }
            ]
        }
        """);
    }
    
    [Fact]
    public async Task Readiness__should_be__ok()
    {
        // await SetupHealth();
        await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var body = await GetProbeAsync(ReadinessUri);

        body.ParseJToken()
            .ReplaceValue("duration", "00:00:00.555")
            .ReplaceValue("checks[*].duration", "00:00:00.555")       // remove randomness
            .Sort("checks")
            .Should().BeEquivalentTo("""
        {
            "status": "healthy",
            "duration": "00:00:00.555",
            "checks": [
                {
                "name": "WeatherContext",
                "duration": "00:00:00.555",
                "status": "healthy",
                "data": {}
                },
            ]
        }
        """);
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    public async Task Readiness__should__depend_on_db(bool dbHealthy, HttpStatusCode heathResult)
    {
        if(dbHealthy)
            await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var body = await GetProbeAsync(ReadinessUri, expected: heathResult);

        // positive body is asserted in Readiness__should_be__ok
        if(dbHealthy)
            return;

        body.ParseJToken()
            .ReplaceValue("duration", "00:00:00.555")
            .ReplaceValue("checks[*].duration", "00:00:00.555")       // remove randomness
            .Sort("checks")
            .Should().BeEquivalentTo("""
        {
            "status": "unhealthy",
            "duration": "00:00:00.555",
            "checks": [
                {
                "name": "WeatherContext",
                "duration": "00:00:00.555",
                "status": "unhealthy",
                "data": {}
                },
            ]
        }
        """);
    }
    
    [Fact]
    public async Task Overview__should_be__ok()
    {
        // await SetupHealth();
        await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var body = await GetProbeAsync(HealthAllUri);

        body.ParseJToken()
            .ReplaceValue("duration", "00:00:00.555")
            .ReplaceValue("checks[*].duration", "00:00:00.555")       // remove randomness
            .Sort("checks")
            .Should().BeEquivalentTo("""
        {
            "status": "healthy",
            "duration": "00:00:00.555",
            "checks": [
                {
                    "name": "redis-conn-2",
                    "description": "Redis Connection is alive.",
                    "duration": "00:00:00.555",
                    "status": "healthy",
                    "data": {}
                },
                {
                    "name": "RedisConnection_For_SignalR",
                    "description": "Redis Connection is alive.",
                    "duration": "00:00:00.555",
                    "status": "healthy",
                    "data": {}
                },
                {
                "name": "SimpleHealthCheck",
                "description": "AspNet is alive.",
                "duration": "00:00:00.555",
                "status": "healthy",
                "data": {}
                },
                {
                "name": "WeatherContext",
                "duration": "00:00:00.555",
                "status": "healthy",
                "data": {}
                },
            ]
        }
        """);
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    public async Task Overview__should__depend_on_db(bool whenHealthy, HttpStatusCode healthcheckResult)
    {
        await SetupCheckedServices(dbHealthy: whenHealthy);

        var body = await GetProbeAsync(HealthAllUri, expected: healthcheckResult);
        
        // positive body is asserted in Overview__should_be__ok
        if(whenHealthy)
            return;

        body.ParseJToken()
            .Should().ContainSubtree("""
        {
            "checks": [
                {
                    "name": "WeatherContext",
                    "status": "unhealthy",
                },
            ]
        }
        """);
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    public async Task Overview__should__depend_on_redis_signalR(bool whenHealthy, HttpStatusCode healthcheckResult)
    {
        await SetupCheckedServices(redisHealthy: whenHealthy);

        var timeout = 1.5;
        if (whenHealthy == false) 
            //timeout = 6;
            timeout = 15;

        var body = await GetProbeAsync(HealthAllUri, expected: healthcheckResult, timeout: timeout);
        
        // positive body is asserted in Overview__should_be__ok
        if(whenHealthy)
            return;

        // we can have one of two errors:
        // 1:
        // "description": "RedisConnectionFactory is starting a connection.",
        // 2:
        // "description": "Redis Connection HealthCheck exception.",
        // "error": "The message timed out in the backlog attempting to send because no connection became available (5000ms) - Last Connection Exception: UnableToConnect on localhost:8080/Interactive, Initializing/NotStarted, last: NONE, origin: BeginConnectAsync, outstanding: 0, last-read: 0s ago, last-write: 0s ago, keep-alive: 60s, state: Connecting, mgr: 10 of 10 available, last-heartbeat: never, global: 0s ago, v: 2.10.1.65101, command=PING, timeout: 5000, inst: 0, qu: 0, qs: 0, aw: False, bw: CheckingForTimeout, last-in: 0, cur-in: 0, sync-ops: 0, async-ops: 2, serverEndpoint: localhost:8080, conn-sec: n/a, aoc: 0, mc: 1/1/0, mgr: 10 of 10 available, clientName: 888ab121f439(SE.Redis-v2.10.1.65101), IOCP: (Busy=0,Free=1000,Min=1,Max=1000), WORKER: (Busy=0,Free=32767,Min=4,Max=32767), POOL: (Threads=6,QueuedItems=0,CompletedItems=295,Timers=13), v: 2.10.1.65101 (Please take a look at this article for some common client-side issues that can cause timeouts: https://stackexchange.github.io/StackExchange.Redis/Timeouts)",
        //
        // anyway we assert error status for this check:
        body.ParseJToken()
            .Should().ContainSubtree("""
        {
            "checks": [
                {
                    "name": "RedisConnection_For_SignalR",
                    "status": "unhealthy",
                },
            ]
        }
        """);
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    public async Task Overview__should__depend_on_redis_conn_2(bool whenHealthy, HttpStatusCode healthcheckResult)
    {
        await SetupCheckedServices(redisHealthy: whenHealthy);

        var timeout = 1.5;
        if (whenHealthy == false) 
            //timeout = 6;
            timeout = 15;

        var body = await GetProbeAsync(HealthAllUri, expected: healthcheckResult, timeout: timeout);
        
        // positive body is asserted in Overview__should_be__ok
        if(whenHealthy)
            return;

        body.ParseJToken()
            .Should().ContainSubtree("""
        {
            "checks": [
                {
                    "name": "redis-conn-2",
                    "status": "unhealthy",
                },
            ]
        }
        """);
    }

    private async Task<string> GetProbeAsync(string uri, double timeout = 1.5, HttpStatusCode expected = HttpStatusCode.OK)
    {
        // warmup
        if(expected == HttpStatusCode.OK)
            _ = await Client.GetAsync(uri);

//TODO: add timeout to TestGetStringAsync ??

        var sw = new Stopwatch();
        sw.Start();
        var body = await Client.TestGetStringAsync(uri, expected);
        sw.Stop();

        sw.Elapsed
                .Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(timeout));

        return body;
    }

    private async Task SetupCheckedServices(bool dbHealthy = true, bool redisHealthy = true)
    {
        if (redisHealthy == false)
            AppBuilder.UseSetting("ConnectionStrings:Redis", "localhost:8080");

        // update builder before app starts
        if(dbHealthy)
            await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }
}