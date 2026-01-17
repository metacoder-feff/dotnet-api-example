using NodaTime;
using NodaTime.Extensions;

using Example.Utils;

namespace Example.Api;

static class ExampleApiModule
{
   internal static void SetupPipeline(WebApplication app)
   {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", (TimeProvider tp, Random rand) =>
        {
            var now = tp.GetCurrentInstant();
            var todayUtc = now.InUtc().LocalDateTime.Date;

            var forecast =  Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    now,
                    (todayUtc + Period.FromDays(index)).ToDateOnly(),
                    rand.Next(-20, 55),
                    summaries[rand.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast");
    }

    internal static void SetupServices(IServiceCollection services)
    {
        services.AddTimeProvider();
        services.AddRandom();
    }
}

record WeatherForecast(Instant Timestamp, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
