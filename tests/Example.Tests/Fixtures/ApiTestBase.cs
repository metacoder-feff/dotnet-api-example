using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Example.Tests;

public class AppFactory : WebApplicationFactoryEx<Program>
{
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
    protected readonly AppFactory _appFactory = new();

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected HttpClient Client
    {
        get
        {
            field ??= _appFactory.CreateClient();
            return field;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _appFactory.DisposeAsync();
    }
}