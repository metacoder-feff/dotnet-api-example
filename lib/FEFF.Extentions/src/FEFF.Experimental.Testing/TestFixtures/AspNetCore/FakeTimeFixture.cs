using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace FEFF.Experimental.TestFixtures.AspNetCore;
using FEFF.Extentions.Testing.AspNetCore;

/// <summary>
/// Replaces <see cref="TimeProvider"/> service with <see cref="FakeTimeProvider"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeTimeFixture
{
    public readonly FakeTimeProvider Value = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    public FakeTimeFixture(ITestApplicationFixture app)
    {
        app.ApplicationBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<TimeProvider>(Value);
    }
}