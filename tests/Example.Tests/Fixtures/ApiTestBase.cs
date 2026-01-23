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

    // props from fixtures for smart access
    protected ITestApplication TestApplication => _appFixture.LazyTestApplication;
    
    protected FakeRandom FakeRandom => _fakes.FakeRandom;

    protected FakeTimeProvider  FakeTime => _fakes.FakeTime;

    /// <summary>
    /// Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected HttpClient Client => _appFixture.LazyClient;

    /// <summary>
    /// Run TestApp, get, memoize and return DbContext instance form TestApp.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= _appFixture.LazyScopeServiceProvider.GetRequiredService<WeatherContext>();
            return field;
        }
    }

    public ApiTestBase()
    {
        // build fixtures tree
        AppBuilder = new TestApplicationBuilder<Program>();
        _appFixture = new(AppBuilder);
        _dbFixture = new(AppBuilder, DbName, InfrastructureModule.PgConnectionStringName);
        _fakes = new FakeServicesFixture(AppBuilder);
    }

//TODO: disposable pattern/DI of 'AppFactory' 
    public async ValueTask DisposeAsync()
    {
        await _appFixture.DisposeAsync();
    }

    /// <summary>
    /// Run TestApp, get, memoize and return TService instance form TestApp.
    /// </summary>
    public TService GetRequiredService<TService>() where TService : notnull =>
        _appFixture.LazyScopeServiceProvider.GetRequiredService<TService>();
}