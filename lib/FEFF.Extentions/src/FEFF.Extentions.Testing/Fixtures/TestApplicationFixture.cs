namespace FEFF.Extentions.Fixtures;
using FEFF.Extentions.Testing;

public interface ITestApplicationFixture
{
    ITestApplicationBuilder ApplicationBuilder  { get; }
    ITestApplication        LazyTestApplication { get; }
}

//TODO: rename
//TODO: async StartServerAsync-> OnStartedHandlerAsync[]
//  e.g. DB.Create
public class TestApplicationFixture<TEntryPoint> : IAsyncDisposable, ITestApplicationFixture
where TEntryPoint: class
{
    private readonly TestApplicationBuilder<TEntryPoint> _appBuilder = new();
    private readonly Lazy<ITestApplication> _app;

    public ITestApplicationBuilder ApplicationBuilder
    {
        get
        {
            if(_app.IsValueCreated)
                throw new InvalidOperationException("Can't use ApplicationBuilder after application is created.");
            return _appBuilder;
        }
    }

    /// <summary>
    /// Creates, memoizes and returns App. The App may be started.<br/>
    /// Access to <see cref="LazyTestApplication"/> finishes app building.
    /// </summary>
    public ITestApplication LazyTestApplication => _app.Value;

    public TestApplicationFixture()
    {
        _app = new (_appBuilder.Build);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app.IsValueCreated)
            await _app.Value.DisposeAsync();
    }
}
