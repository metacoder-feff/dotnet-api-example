using System.Text.Json.Nodes;
using Example.Api.Tests.Fixtures;

namespace Example.Api.Tests;

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
        Assert.InRange((int)response.StatusCode, 200, 299);
        Assert.Equal("application/json; charset=utf-8",
                     response.Content?.Headers?.ContentType?.ToString()
        );

        // Act
        var body = await response.Content?.ReadAsStringAsync()!;
        var json = JsonNode.Parse(body)!;

        // Assert
        var arr = (JsonArray)json;
        Assert.Equal(5, arr.Count);
        Assert.All(arr, (val, idx) =>
        {
            var dict = (JsonObject)val!;
            //Console.WriteLine(wfc);

            var dateStr = DateTime.Now.AddDays(idx +1).ToString("yyyy-MM-dd");

            Assert.Equal(dateStr, (string)dict["date"]!);
            Assert.InRange  (   (int)dict["temperature_c"]!, -30, 60);
            Assert.InRange  (   (int)dict["temperature_f"]!, -15, 150);
            Assert.NotEmpty ((string)dict["summary"]!);   

            // no more props
            Assert.Equal(4, dict.Count);
        });
    }
}