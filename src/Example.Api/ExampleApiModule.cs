using Example.Api.SignalR;
using NodaTime.Extensions;

using FEFF.Extentions.Redis;

namespace Example.Api;

static class ExampleApiModule
{
    internal static void SetupPipeline(IEndpointRouteBuilder app)
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

    private static async Task<WeatherForecast[]> Get(ILogger<EventSender> logger,TimeProvider tp, Random rand, IEventSender sender, IRedisDatabaseProvider redis)
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

//TODO: BL + test + KeyPrefix
        var db = await redis.GetDatabaseAsync();
        var cmd = await db.ListLeftPopAsync("commands");
        logger.LogDebug("cmd:{}", cmd);

        await sender.SendFinishedOkAsync();

        return forecast;
    }
}

record WeatherForecast(Instant Timestamp, LocalDate Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
