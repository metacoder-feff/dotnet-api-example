using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace FEFF.Extentions.Fixtures;
using FEFF.Extentions.Testing;

[Fixture]
public class FakeTimeFixture
{
    public readonly FakeTimeProvider FakeTime = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    public FakeTimeFixture(ITestApplicationFixture app)
    {
        app.ApplicationBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<TimeProvider>(FakeTime);
    }
}