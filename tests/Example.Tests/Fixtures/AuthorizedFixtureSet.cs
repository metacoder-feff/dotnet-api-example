using FEFF.TestFixtures.AspNetCore.Preview;
using FEFF.TestFixtures.AspNetCore.SignalR;

namespace Example.Tests.Fixures;

[Fixture]
public record AuthorizedFixtureSet
(
    FixtureSet BaseFx, // ensure base fixtures are invoked (e.g. DB rename/delete)
    AuthorizedAppClientFixture<Program, FixtureOptions> ClientFx,
    SignalrClientFixture<Program, FixtureOptions> SignalR
);