namespace FEFF.Extentions.Testing;

[Fixture]
public sealed class ClientFixture : IDisposable
{
    private readonly Lazy<HttpClient> _client;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    public HttpClient Client => _client.Value;

    public ClientFixture(ITestApplicationFixture app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _client = new(() => app.LazyTestApplication.CreateClient());
    }

    public void Dispose()
    {
        if(_client.IsValueCreated)
            _client.Value.Dispose();
    }
}
