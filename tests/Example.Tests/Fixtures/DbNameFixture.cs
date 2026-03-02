using FEFF.Experimental.TestFixtures;
using FEFF.Experimental.TestFixtures.AspNetCore;

namespace Example.Tests.Fixures;
using Example.Api;

[Fixture]
public class DbNameFixture(ITestApplicationFixture app, TestIdFixture testId) 
    : DbNameFixtureBase(app, testId, InfrastructureModule.PgConnectionStringName)
{
}