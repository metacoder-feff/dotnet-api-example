using FEFF.Experimental.TestFixtures;

namespace Xunit.v3;

public static class TestContextExtentions
{
    public static FixtureContainer GetFixtureContainer(this ITestContext ctx)
    {
        return FixturesXUnitExtensionAttribute.GetFixtureContainer(ctx);
    }
}