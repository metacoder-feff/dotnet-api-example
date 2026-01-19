using System.Diagnostics;
using System.Net;

namespace Example.Tests;

public class HealthCheckTest : ApiTestBase
{
    private const string LivenesstUri = "/health/liveness";
    private const string ReadinessUri = "/health/readiness";
    private const string HealthAllUri = "/health/overal";

    [Fact]
    public async Task Health_Liveness_should_be_ok()
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
    public async Task Health_Readiness__should_be__ok()
    {
        // await SetupHealth();
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

    // [Fact]
    // public async Task Health_Readiness__should_be__500_unhealthy_db()
    // {
    // }
    
    [Fact]
    public async Task Health_Overal__should_be__ok()
    {
        // await SetupHealth();
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

    // [Fact]
    // public async Task Health_Overal__should_be__500_unhealthy_redis()
    // {
    // }
    
    private async Task<string> GetProbeAsync(string uri, double timeout = 1.5, HttpStatusCode expected = HttpStatusCode.OK)
    {
// TODO: warmup
        //_ = await Client.GetAsync(uri);

        var sw = new Stopwatch();
        sw.Start();
        var body = await Client.GetStringAsync(uri, expected);
        sw.Stop();

        sw.Elapsed
            .Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(timeout));

        return body;
    }
}