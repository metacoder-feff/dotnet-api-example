using Example.Api;

namespace Example.Tests.Fixures;

[Fixture]
public class DbNameFixture(ITestApplicationFixture app, TestIdFixture testId) 
    : DbNameFixtureBase(app, testId, InfrastructureModule.PgConnectionStringName)
{
}