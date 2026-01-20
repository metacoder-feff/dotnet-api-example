using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Example.Api;

namespace Example.Tests;

//TODO: disposable pattern/DI of 'AppFactory' 
public class ApiTestBase: IAsyncDisposable
{
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    private readonly Lazy<AsyncServiceScope> _scope;
    public readonly string DbName = $"Weather-test-{Guid.NewGuid()}";

    protected WebApplicationFactoryEx<Program> Factory {get; } = new();

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected HttpClient Client
    {
        get
        {
            field ??= Factory.CreateClient();
            return field;
        }
    }

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns ServiceScope.
    /// </summary>
    protected AsyncServiceScope Scope => _scope.Value;
    
    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected WeatherContext DbCtx
    {
//TODO: SingleFixture        
        get
        {
            field ??= GetScopedRequiredService<WeatherContext>();
            return field;
        }
    }

    public ApiTestBase()
    {
        _scope = new(() => Factory.Services.CreateAsyncScope());
        Factory.BuilderOverrider.ConfigureServices(ReconfigureFactory);
        
//TODO: devcontainer settings, dockerfile ENV & CI job env??
        // WORKAROUND for linux error:
        //   "The configured user limit (128) on the number of inotify..."
        Factory.BuilderOverrider.UseSetting("DOTNET_hostBuilder:reloadConfigOnChange", "false");
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

    public async ValueTask DisposeAsync()
    {
        // need to delete at all ?
        // or just leave ?
        // or intellegent backround batch delete after a number of tests finished?
        //await TryDeleteDatabaseAsync();

        //TODO: (warning) multithreaded error
        if (_scope.IsValueCreated)
            await _scope.Value.DisposeAsync();

        await Factory.DisposeAsync();
    }

    private async Task TryDeleteDatabaseAsync()
    {
//TODO: (optimization) check if app is started

        // e.g. App cannot be started in a negative test
        try
        {
            await DbCtx.Database.EnsureDeletedAsync();
        }
        catch { }
    }

    public T GetScopedRequiredService<T>() where T : notnull =>
        Scope.ServiceProvider.GetRequiredService<T>();

    public T GetRequiredService<T>() where T : notnull =>
        Factory.Services.GetRequiredService<T>();
}