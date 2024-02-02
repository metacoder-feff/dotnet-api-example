using Example.Api.Tests.Fixtures;

namespace Example.Api.Tests;

public class InfrastructureTest : IntegrationTest
{
    public InfrastructureTest(ApiWebApplicationFactory fixture)
        : base(fixture) 
    {
    }

    [Fact]
    public async Task GetHealth()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var body = await response.Content?.ReadAsStringAsync()!;

        // Assert
        Assert.InRange((int)response.StatusCode, 200, 299);
        Assert.Equal("Healthy", body);
    }
}