using System.ComponentModel.DataAnnotations;
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

public class Forecast : EntityBase
{
    public string?  Name    { get; set; }
    public long     Count   { get; set; }
    //public Instant CreatedAt {get; set;}
}

public class EntityBase
{
    // PG recomends 'long' for optimisation purposes
    public /*auto*/ long    Id          { get; init; }

    // optimistic concurrency token
    // EF automatically adds predicate to update statement

    // available for pgsql - db-generated
    // [Timestamp]
    // public uint Version { get; init; }

    // available for msSql - db-generated
    // [Timestamp]
    // public byte[] Version { get; init; } = [];

    // available everywhere - application should update this propery ?
    // (better to create SaveChangesInterceptor)
    // [ConcurrencyCheck]
    // public Guid Version { get; init; }

    [Timestamp]
    public /*auto*/ uint    Version     { get; init; }

    //UpdatedAtInterceptor
    public /*auto*/ Instant CreatedAt   { get; init; }
    public /*auto*/ Instant UpdatedAt   { get; init; }
}