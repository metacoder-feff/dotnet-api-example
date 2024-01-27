using MegaBack.Api.Tests.Fixtures;

namespace MegaBack.Api.Tests;

public class WeatherForecastTests : IntegrationTest
{
    public WeatherForecastTests(ApiWebApplicationFactory fixture)
        : base(fixture) 
    {
    }

    [Theory]
    [InlineData("/api/v1/weather_forecast_static")]
    [InlineData("/api/v1/weather_forecast_io_bound")]
    [InlineData("/api/v1/weather_forecast_cpu_bound")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("application/json; charset=utf-8",
                    response.Content?.Headers?.ContentType?.ToString());
    }
}