using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Fixtures;
using FEFF.Extentions.Testing;
using FEFF.Extentions.Testing.AspNetCore;

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