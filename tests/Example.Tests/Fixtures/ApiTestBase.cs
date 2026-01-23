using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Example.Api;

namespace Example.Tests;

public class ApiTestBase: IAsyncDisposable
{
    private readonly string DbName = $"Weather-test-{Guid.NewGuid()}";

    protected ITestApplicationBuilder AppBuilder {get; }
    private readonly TestApplicationFixture _appFixture;
    private readonly TestDbFixture _dbFixture;
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    protected ITestApplication App => _appFixture.LazyApp;
    protected HttpClient Client => _appFixture.LazyClient;

    public ApiTestBase()
    {
        AppBuilder = new TestApplicationBuilder<Program>();
        _appFixture = new(AppBuilder);
        _dbFixture = new(AppBuilder, DbName, InfrastructureModule.PgConnectionStringName);
        AppBuilder.ConfigureServices(ReconfigureFactory);
    }

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