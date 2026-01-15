using System.Diagnostics;

namespace Example.Tests;

public class HealthCheckTest : ApiTestBase
{
    private const string LivenesstUri = "/health/liveness";
    private const string ReadinessUri = "/health/readiness";
    private const string HealthAllUri = "/health/overal";

    [Fact]
    public async Task Health_Liveness_should_be_ok()
    {
        // warmup
        _ = await Client.GetStringAsync(LivenesstUri);

        var sw = new Stopwatch();
        sw.Start();
        var body = await Client.GetStringAsync(LivenesstUri);
        sw.Stop();

        sw.Elapsed
            .Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(0.5));        

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

    // [Fact]
    // public async Task Health_Readiness__should_be__500_unhealthy()
    // {
    // }

    // [Fact]
    // public async Task Health_Readiness__should_be__ok()
    // {
    // }
    
    // [Fact]
    // public async Task Health_Overal__should_be__ok()
    // {
    // }

    // [Fact]
    // public async Task Health_Overal__should_be__503_unhealthy()
    // {
    // }
}