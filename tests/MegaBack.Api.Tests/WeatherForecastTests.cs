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

//TODO: assert all
        var first = (JsonObject)arr.First()!;
        //Console.WriteLine(first);   
        Assert.Equal(4, first.Count);

        //var tomorrowStr = DateOnly.FromDateTime(DateTime.Now.AddDays(1)).ToShortDateString();
        var tomorrowStr = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        Assert.Equal(tomorrowStr, (string)first["date"]!);
        Assert.InRange  ((int)   first["temperatureC"]!, -50, 50);
        Assert.InRange  ((int)   first["temperatureF"]!, -15, 150);
        Assert.NotEmpty ((string)first["summary"]!);        
    }
}