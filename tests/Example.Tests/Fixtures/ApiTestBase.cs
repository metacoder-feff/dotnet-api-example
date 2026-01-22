using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Example.Api;

namespace Example.Tests;


public class DbContextFixture
{
    private readonly string DbName = $"Weather-test-{Guid.NewGuid()}";
    private readonly WebApplicationFixture<Program> _appFixture;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= _appFixture.LazyScopeServiceProvider.GetRequiredService<WeatherContext>();
            return field;
        }
    }

    public DbContextFixture(WebApplicationFixture<Program> appFixture)
    {
        _appFixture = appFixture;
        _appFixture.AppBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(WebHostBuilderContext ctx, IServiceCollection _)
    {
        
        var config = (ConfigurationManager)ctx.Configuration;
//TODO: const
        //var key = "ConnectionStrings:" + Setup.ConfigNames.PgConnectionString;
        var key = "ConnectionStrings:" + "PgDb";
        ChangeDbName(config, key);
    }
    
    private void ChangeDbName(ConfigurationManager config, string key)
    {
        var cs = config[key];
        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = cs
        };

        csb["Database"] = DbName;
        var newCs = csb.ConnectionString;
        config[key] = newCs;
    }

    // public async ValueTask DisposeAsync()
    // {
        // need to delete at all ?
        // or just leave ?
        // or intellegent backround batch delete after a number of tests finished?
        //await TryDeleteDatabaseAsync();
    // }
    
//TODO: delete without DbCtx
//     private async Task TryDeleteDatabaseAsync()
//     {
// //TODO: (optimization) check if app is started
//         // e.g. App cannot be started in a negative test
//         try
//         {
//             await DbCtx.Database.EnsureDeletedAsync();
//         }
//         catch { }
//     }
}

public class ApiTestBase: IAsyncDisposable
{
    private readonly WebApplicationFixture<Program> _appFixture = new();
    private readonly DbContextFixture _dbFixture;
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    protected TestingAppBuilder AppBuilder => _appFixture.AppBuilder;
    protected WebApplicationFactory<Program> App => _appFixture.LazyApp;
    protected HttpClient Client => _appFixture.LazyClient;
    protected WeatherContext DbCtx => _dbFixture.DbCtx;

    public ApiTestBase()
    {
        _dbFixture = new(_appFixture);
        AppBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<Random>(FakeRandom);
        services.TryReplaceSingleton<TimeProvider>(FakeTime);
    }

//TODO: disposable pattern/DI of 'AppFactory' 
    public async ValueTask DisposeAsync()
    {
        await _appFixture.DisposeAsync();
    }

    public T GetRequiredService<T>() where T : notnull =>
        _appFixture.LazyScopeServiceProvider.GetRequiredService<T>();
}