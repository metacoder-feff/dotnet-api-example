namespace FEFF.Extentions.Web;

public sealed class SimpleLogContext : IDisposable
{
    private readonly ServiceProvider _provider;

    public ILogger Logger { get; }

    public SimpleLogContext(Action<ILoggingBuilder> configureLogging, string logCategoryName)
    {
        var sc = new ServiceCollection();
        sc.AddLogging(configureLogging);

        _provider = sc.BuildServiceProvider();

        Logger = _provider.GetRequiredService<ILoggerFactory>().CreateLogger(logCategoryName);
    }

    public void Dispose()
    {
        _provider.Dispose();
        GC.SuppressFinalize(this);
    }
}