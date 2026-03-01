using FEFF.Extentions.Fixtures;

namespace Example.Tests.Fixures;

// register this fixture as ITestApplicationFixture to allow 
// other fixtures (from other libs) to access it by interface
[Fixture(FixtureType = typeof(ITestApplicationFixture))]
public class TestApplicationFixture : TestApplicationFixture<Program>
{
}