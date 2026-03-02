using FEFF.Experimental.TestFixtures;

namespace Xunit.v3;

public static class TestContextExtentions
{
    public static IFixtureProvider GetTestCaseFixtureProvider(this ITestContext ctx)
    {
        return TestCaseFixturesXUnitExtensionAttribute.GetTestCaseFixtureProvider(ctx);
    }
}