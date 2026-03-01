using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

[Fixture]
public class FakeRandomFixture
{
    public readonly FakeRandom FakeRandom = new();

    public FakeRandomFixture(ITestApplicationFixture app)
    {
        app.ApplicationBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<Random>(FakeRandom);
    }
}