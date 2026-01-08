using Example.Api;
using FEFF.Extensions.Jwt;
using FEFF.TestFixtures.AspNetCore.Preview;
using FEFF.TestFixtures.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Tests.Fixures;

[Fixture]
public class FixtureOptions(AppServicesFixture<Program> svc) : IAuthorizedClientFixtureOptions, ITmpDatabaseNameFixtureOptions, ISignalrClientFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => [InfrastructureModule.PgConnectionStringName];
    public string SignalrApiPath => "/api/v1/public/events";
    public string GetJwt()
    {
        var jwt = svc.LazyServiceProvider.GetRequiredService<IJwtFactory>();
        return LoginApiModule.CreateToken(jwt, "testuser");
    }
}