
namespace Example.Tests.InfrastructrureTests;

public class MetricsApiTest : ApiTestBase
{
    [Fact]
    public async Task Metrics_should_return()
    {
        var body = await Client.TestGetStringAsync("/metrics");

        body.Should().Contain("dotnet_collection_count_total counter");
    }
}