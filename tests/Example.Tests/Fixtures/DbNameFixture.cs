using Example.Api;
using FEFF.Extentions.Fixtures;

namespace Example.Tests.Fixures;

[Fixture]
public class DbNameFixture(ITestApplicationFixture app, TestIdFixture testId) 
    : DbNameFixtureBase(app, testId, InfrastructureModule.PgConnectionStringName)
{
}