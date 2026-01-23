using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Example.Api;

namespace Example.Tests;

public class ApiTestBase: IAsyncDisposable
{
    private readonly string DbName = $"Weather-test-{Guid.NewGuid()}";

    // fixures
    protected ITestApplicationBuilder AppBuilder {get; }
    private readonly TestApplicationFixture _appFixture;
    private readonly TestDbFixture _dbFixture;
    private readonly FakeServicesFixture _fakes;

    // props from fixtures
    protected ITestApplication App => _appFixture.LazyApp;
    protected HttpClient Client => _appFixture.LazyClient;
    protected FakeRandom FakeRandom => _fakes.FakeRandom;
    protected FakeTimeProvider  FakeTime => _fakes.FakeTime;

    public ApiTestBase()
    {
        AppBuilder = new TestApplicationBuilder<Program>();
        _appFixture = new(AppBuilder);
        _dbFixture = new(AppBuilder, DbName, InfrastructureModule.PgConnectionStringName);
        _fakes = new FakeServicesFixture(AppBuilder);
    }

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns DbContext.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= _appFixture.LazyScopeServiceProvider.GetRequiredService<WeatherContext>();
            return field;
        }
    }

//TODO: disposable pattern/DI of 'AppFactory' 
    public async ValueTask DisposeAsync()
    {
        await _appFixture.DisposeAsync();
    }

    public T GetRequiredService<T>() where T : notnull =>
        _appFixture.LazyScopeServiceProvider.GetRequiredService<T>();
}