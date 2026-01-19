using Microsoft.EntityFrameworkCore;

namespace Example.Api;

//TODO: .AsNoTracking() by default

/// <summary>
/// npgsql (Postgres driver for EF)
/// 'ID (auto-increment)' recomendations => Type = 'long':
/// 1. https://www.npgsql.org/efcore/modeling/generated-properties.html?tabs=13%2Cefcore5
/// 2. https://www.npgsql.org/doc/types/basic.html
/// </summary>
public class WeatherContext : DbContext
{
    public WeatherContext(DbContextOptions<WeatherContext> options)
        : base(options)
    {
    }

    public required DbSet<Forecast>      Forecasts      { get; set; }// = null!;
}

public class Forecast
{
    public long Id {get; set;}
    //public Instant CreatedAt {get; set;}
}