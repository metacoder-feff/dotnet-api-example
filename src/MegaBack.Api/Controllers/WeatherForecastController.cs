using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
//using Asp.Versioning;

namespace MegaBack.Api.Controllers;

//[Route("[controller]")]
//[ApiVersion("0.1")]
//[Route("api/v{version:apiVersion}/[controller]/[action]")]
[Route("api/v1")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly Random _rand = new Random();

    private const int DelayMs = 5;
    //private const int DelayMs = 2000;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("weather_forecast_static")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("weather_forecast_io_bound")]
    public async Task<ActionResult<WeatherForecast1[]>> GetIoBound()
    {
        //_context.TodoItems.Add(todoItem);
        //await _context.SaveChangesAsync();

        await Task.Delay(DelayMs);

        var r = await Task.Run( () =>
        {
            var forecast =  Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast1
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToArray();
            return forecast;
        });
        return r;
    }
    

    [HttpGet("weather_forecast_cpu_bound")]
    public async Task<ActionResult<WeatherForecast1[]>> GetCpuBound()
    {
        //_context.TodoItems.Add(todoItem);
        //await _context.SaveChangesAsync();

        var r = await Task.Run( () =>
        {
            int res = 0;
            var sw = new Stopwatch();
            sw.Start();
            while(sw.ElapsedMilliseconds < DelayMs)
            {
                var x = FindPrimeNumber(_rand.Next(1000) + 1);
                res += (int) x;
            }

            var forecast =  Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast1
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                res,
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToArray();
            return forecast;
        });
        return r;
    }

    private static long FindPrimeNumber(int n)
    {
        int count=0;
        long a = 2;
        while(count<n)
        {
            long b = 2;
            int prime = 1;// to check if found a prime
            while(b * b <= a)
            {
                if(a % b == 0)
                {
                    prime = 0;
                    break;
                }
                b++;
            }
            if(prime > 0)
            {
                count++;
            }
            a++;
        }
        return (--a);
    }
}
