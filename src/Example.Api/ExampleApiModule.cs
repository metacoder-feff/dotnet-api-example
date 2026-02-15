using Example.Api.SignalR;
using NodaTime.Extensions;

namespace Example.Api;

static class ExampleApiModule
{
   internal static void SetupPipeline(WebApplication app)
   {
        app.MapGet("/weatherforecast", Get)
            .WithName("GetWeatherForecast");
    }

    internal static void SetupServices(IServiceCollection services)
    {
        services.AddTimeProvider();
        services.AddRandom();
        services.AddTransient<IEventSender, EventSender>();
    }

    private static async Task<WeatherForecast[]> Get(TimeProvider tp, Random rand, IEventSender sender)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var now = tp.GetCurrentInstant();
        var todayUtc = now.InUtc().LocalDateTime.Date;

        var forecast =  Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                now,
                todayUtc + Period.FromDays(index),
                rand.Next(-20, 55),
                summaries[rand.Next(summaries.Length)]
            ))
            .ToArray();

        await sender.SendFinishedOkAsync();

        return forecast;
    }
}

record WeatherForecast(Instant Timestamp, LocalDate Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
