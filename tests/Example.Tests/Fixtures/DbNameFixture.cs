using Example.Api;

namespace Example.Tests.Fixures;

public class DbNameFixture : DbNameFixtureBase
{
    public DbNameFixture(ITestApplicationBuilder appBuilder, TestIdFixture testId) 
    : base(appBuilder, testId, InfrastructureModule.PgConnectionStringName)
    {
    }
}