using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

[Fixture]
public sealed class AppServiceScopeFixture : IAsyncDisposable
{
    private readonly Lazy<AsyncServiceScope> _appServiceScope;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns ServiceScope.
    /// </summary>
    public IServiceProvider LazyScopeServiceProvider => _appServiceScope.Value.ServiceProvider;

    public AppServiceScopeFixture(ITestApplicationFixture app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _appServiceScope = new(() => app.LazyTestApplication.Services.CreateAsyncScope());
    }

    public ValueTask DisposeAsync()
    {
        if(_appServiceScope.IsValueCreated)
            return _appServiceScope.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
    
    public T GetRequiredService<T>() where T : notnull =>
        LazyScopeServiceProvider.GetRequiredService<T>();
}
