using Microsoft.AspNetCore.Mvc.Testing;

namespace MegaBack.Api.Tests;

public class BasicTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    //[InlineData("/")]
    [InlineData("/api/v1/weather_forecast_static")]
    [InlineData("/api/v1/weather_forecast_io_bound")]
    [InlineData("/api/v1/weather_forecast_cpu_bound")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("application/json; charset=utf-8",
                    response.Content?.Headers?.ContentType?.ToString());
    }
}