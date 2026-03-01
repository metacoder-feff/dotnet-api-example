using Example.Api;

namespace Example.Tests.Fixures;

[Fixture]
public class DbNameFixture : DbNameFixtureBase
{
    public DbNameFixture(ITestApplicationBuilder appBuilder, TestIdFixture testId) 
    : base(appBuilder, testId, InfrastructureModule.PgConnectionStringName)
    {
    }
}