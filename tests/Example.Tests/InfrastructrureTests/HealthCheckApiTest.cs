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

        if(dbHealthy)
            return;
        
        // Assert Error Body

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
    public async Task Overview__should__depend_on_db(bool dbHealthy, HttpStatusCode heathResult)
    {
        if(dbHealthy)
            await DbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        _ = await GetProbeAsync(HealthAllUri, expected: heathResult);
    }

    // [Fact]
    // public async Task Overview__should_be__500_unhealthy_redis()
    // {
    // }
    
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
}