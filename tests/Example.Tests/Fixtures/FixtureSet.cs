using Example.Api;
using FEFF.Experimental.TestFixtures.AspNetCore;
using FEFF.Extensions.Redis;
using FEFF.Extensions.SignalR.Redis;
using FEFF.TestFixtures.AspNetCore.EF;

namespace Example.Tests.Fixures;

/// <summary>
/// This fixures are required by most of tests
/// </summary>
[Fixture]
public record FixtureSet(
    AppManagerFixture<Program> TestApplication,
    FakeRandomFixture<Program> FakeRandom,
    FakeTimeFixture<Program> FakeTime,
    AppClientFixture<Program> ClientFx,
    AppServicesFixture<Program> ServiceFx,
    TmpDatabaseNameFixture<Program, FixtureOptions> TmpDbName,
    DatabaseLifecycleFixture<Program, WeatherContext> EnsureDb,

    // channel prefix for SignalR redis connection
    RedisChannelPrefixFixture<Program, SignalRedisProviderProxy> SignalRedisPrefix,
    // KeyPrefix and ChannelPrefix for main redis connection
    TmpRedisPrefixFixture<Program, RedisConnectionManager> SecondRedisPrefix
);