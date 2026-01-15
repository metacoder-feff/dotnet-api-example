using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;

namespace Example.Tests;

public class AppFactory : WebApplicationFactoryEx<Program>
{
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((ctx, configBuilder) =>
        {
            //configBuilder.AddInMemoryCollection(AdditionalSettings);

            // WORKAROUND for vscode error:
            //   "The configured user limit (128) on the number of inotify..."
            //Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "false");
            var ccc = configBuilder.Sources.OfType<FileConfigurationSource>();
            foreach (var x in ccc)
            {
                x.ReloadOnChange = false;
            }
        });

        
        builder.ConfigureServices( (ctx, services) =>
        {
            services.TryReplaceSingleton<Random>(FakeRandom);
            services.TryReplaceSingleton<TimeProvider>(FakeTime);
        });
    }
}

//TODO: disposable pattern/DI of 'AppFactory' 
public class ApiTestBase: IAsyncDisposable
{
    protected readonly AppFactory _appFactory = new();

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected XUnitHttpClient Client
    {
        get
        {
            field ??= _appFactory.CreateTestClient();
            return field;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _appFactory.DisposeAsync();
    }
}