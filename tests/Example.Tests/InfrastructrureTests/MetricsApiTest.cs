using System.Net;

namespace Example.Tests;

public class MetricsApiTest : ApiTestBase
{
    [Fact]
    public async Task Metrics_should_return()
    {
        var body = await Client.GetStringAsync("/metrics");

        body.Should().Contain("dotnet_collection_count_total counter");
    }
}