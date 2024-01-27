using System.Text.Json.Nodes;
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

        var body = await response.Content?.ReadAsStringAsync()!;
        var json = JsonNode.Parse(body)!;

        var arr = (JsonArray)json;
        Assert.Equal(5, arr.Count);
        Assert.All(arr, (val, idx) =>
        {
            var wfc = (JsonObject)val!;
            //Console.WriteLine(wfc);

            var dateStr = DateTime.Now.AddDays(idx +1).ToString("yyyy-MM-dd");

            Assert.Equal(dateStr, (string)wfc["date"]!);
            Assert.InRange  (   (int)wfc["temperature_c"]!, -30, 60);
            Assert.InRange  (   (int)wfc["temperature_f"]!, -15, 150);
            Assert.NotEmpty ((string)wfc["summary"]!);   

            // no more props
            Assert.Equal(4, wfc.Count);
        });
    }
}