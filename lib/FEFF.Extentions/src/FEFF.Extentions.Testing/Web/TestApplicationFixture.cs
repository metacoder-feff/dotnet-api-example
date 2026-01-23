using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

//TODO: own ITestApplicationBuilder ???
public sealed class TestApplicationFixture : IAsyncDisposable
{
    private readonly Lazy<ITestApplication> _app;
    private readonly Lazy<AsyncServiceScope> _appServiceScope;

    //public TestingAppBuilder AppBuilder {get; } = new();

    /// <summary>
    /// Creates, memoizes and returns App. The App may be started.
    /// </summary>
    public ITestApplication LazyApp => _app.Value;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    public HttpClient LazyClient
    {
        get
        {
            field ??= LazyApp.CreateClient();
            return field;
        }
    }

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns ServiceScope.
    /// </summary>
    public IServiceProvider LazyScopeServiceProvider => _appServiceScope.Value.ServiceProvider;

    public TestApplicationFixture(ITestApplicationBuilder appBuilder)
    {
        _app = new (() => appBuilder.Build());
        // cannot remove lambda expression because acces to 'App.Services' starts an app
        // but we only need to register callback
        _appServiceScope = new(() => LazyApp.Services.CreateAsyncScope()); 
    }

    public async ValueTask DisposeAsync()
    {
//TODO: (warning) multithreaded error
        if (_appServiceScope.IsValueCreated)
            await _appServiceScope.Value.DisposeAsync();

        if (_app.IsValueCreated)
            await _app.Value.DisposeAsync();
    }

    public T GetRequiredService<T>() where T : notnull =>
        LazyScopeServiceProvider.GetRequiredService<T>();
}
