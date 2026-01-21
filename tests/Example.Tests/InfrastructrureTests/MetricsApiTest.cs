
namespace Example.Tests.InfrastructrureTests;

public class MetricsApiTest : ApiTestBase
{
    [Fact]
    public async Task Metrics__should__return_counter()
    {
        var body = await Client.TestGetStringAsync("/metrics");

        body.Should().Contain("dotnet_collection_count_total counter");
    }
}