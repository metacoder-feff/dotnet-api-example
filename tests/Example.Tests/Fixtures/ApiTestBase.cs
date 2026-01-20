using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Example.Api;

namespace Example.Tests;

//TODO: to ApiTestBase
public class AppFactory : WebApplicationFactoryEx<Program>
{
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
//TODO: devcontainer settings, dockerfile ENV & CI job env??
        // WORKAROUND for linux error:
        //   "The configured user limit (128) on the number of inotify..."
        builder.UseSetting("DOTNET_hostBuilder:reloadConfigOnChange", "false");
        // System ENV: DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE
        
        builder.ConfigureServices( (ctx, services) =>
        {
            services.TryReplaceSingleton<Random>(FakeRandom);
            services.TryReplaceSingleton<TimeProvider>(FakeTime);
        });

        // override by delegates
        base.ConfigureWebHost(builder);
    }
}

//TODO: disposable pattern/DI of 'AppFactory' 
public class ApiTestBase: IAsyncDisposable
{
    private readonly Lazy<AsyncServiceScope> _scope;
    public readonly string DbName = $"Weather-test-{Guid.NewGuid()}"; //.NewGuid().ToString();

    protected AppFactory Factory {get; } = new();

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
    }

    private void ReconfigureFactory(WebHostBuilderContext ctx, IServiceCollection services)
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

    public async ValueTask DisposeAsync()
    {
        await TryDeleteDatabaseAsync();

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