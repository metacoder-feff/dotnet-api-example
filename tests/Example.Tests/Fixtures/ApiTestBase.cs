using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Example.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Example.Tests;

public class ApiTestBase: IAsyncDisposable
{
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    private readonly Lazy<AsyncServiceScope> _appServiceScope;
    private readonly Lazy<WebApplicationFactory<Program>> _app;
    public readonly string DbName = $"Weather-test-{Guid.NewGuid()}";

    protected TestingAppBuilder AppBuilder {get; } = new();

    /// <summary>
    /// Creates, memoizes and returns App. The App may be started.
    /// </summary>
    protected WebApplicationFactory<Program> App => _app.Value;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected HttpClient Client
    {
        get
        {
            field ??= App.CreateClient();
            return field;
        }
    }

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns ServiceScope.
    /// </summary>
    protected AsyncServiceScope Scope => _appServiceScope.Value;
    
    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected WeatherContext DbCtx
    {
//TODO: SingleFixture        
        get
        {
            field ??= GetRequiredService<WeatherContext>();
            return field;
        }
    }

    public ApiTestBase()
    {
        _app = new (() =>AppBuilder.CreateApp<Program>());
        // cannot remove lambda expression because acces to 'App.Services' starts an app
        // but we only need to register callback
        _appServiceScope = new(() => App.Services.CreateAsyncScope()); 
        AppBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(WebHostBuilderContext ctx, IServiceCollection services)
    {
        services.TryReplaceSingleton<Random>(FakeRandom);
        services.TryReplaceSingleton<TimeProvider>(FakeTime);
        
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

//TODO: disposable pattern/DI of 'AppFactory' 
    public async ValueTask DisposeAsync()
    {
        // need to delete at all ?
        // or just leave ?
        // or intellegent backround batch delete after a number of tests finished?
        //await TryDeleteDatabaseAsync();

        //TODO: (warning) multithreaded error
        if (_appServiceScope.IsValueCreated)
            await _appServiceScope.Value.DisposeAsync();

        if (_app.IsValueCreated)
            await _app.Value.DisposeAsync();
    }

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

    public T GetRequiredService<T>() where T : notnull =>
        Scope.ServiceProvider.GetRequiredService<T>();
}