using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace FEFF.Extentions.Testing;

public class FakeServicesFixture
{
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    public FakeServicesFixture(ITestApplicationBuilder appBuilder)
    {
        appBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<Random>(FakeRandom);
        services.TryReplaceSingleton<TimeProvider>(FakeTime);
    }
}