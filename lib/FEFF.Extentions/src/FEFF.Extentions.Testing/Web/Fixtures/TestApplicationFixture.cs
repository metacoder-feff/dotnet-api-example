namespace FEFF.Extentions.Testing;

//TODO: own ITestApplicationBuilder ???
//TODO: async StartServerAsync-> OnStartedHandlerAsync[]
//  e.g. DB.Create
[Fixture]
public sealed class TestApplicationFixture : IAsyncDisposable
{
    private readonly Lazy<ITestApplication> _app;

//TODO: rename
    /// <summary>
    /// Creates, memoizes and returns App. The App may be started.<br/>
    /// Access to <see cref="LazyTestApplication"/> finishes app building.
    /// </summary>
    public ITestApplication LazyTestApplication => _app.Value;

    public TestApplicationFixture(ITestApplicationBuilder appBuilder)
    {
        _app = new (appBuilder.Build);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app.IsValueCreated)
            await _app.Value.DisposeAsync();
    }
}
