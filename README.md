# dotnet-api-example
## Tooling:
+ DevContainers:
  + dotnet-sdk-10
  + postgres
  + valkey (redis)
  + minio (S3)


## WebApp Infrastructure:
+ NodaTime
+ Minimal Api: ConfigureJsonSerializer
  + snake_case
  + JsonStringEnumConverter
  + NodaTime integration
+ DB: postgres + tests
  + NodaTime integration
  + HealthCheck (readiness)
  + Optimistic lock (concurrency exception)
  + Automate CreatedAt/UpdatedAt
+ JWT Auth + tests
  + login returns token
  + token validation middleware
  + configuration + options validation
+ appsettings.secrets.json + gitignore
+ openapi/swagger-ui only in DEV + tests
  + NodaTime integration
  + JWT integration
+ prometheus metrics + tests
+ cloud-compatible healthchecks + tests
  + liveness
  + readiness
  + overview
  + ConfigureJsonSerializer
+ cloud-compatible logging + tests
  + stdout
  + json-lines
  + timestamp format
+ exceptions at 'WebApplication.Run()' are logged + tests
+ exceptions at 'WebApplicationBuilder.Build()' are logged + tests
+ Redis + tests
  + BL example
  + DI container: Singleton 'RedisConnectionManager'
  + DI container: ConfigureOptions using ConnectionString + delegate
  + RedisConnection HealthCheck (overview)
+ SignalR + tests
  + ConfigureJsonSerializer
  + Redis (via DI container)
  + RedisConnection HealthCheck (overview)
  + Jwt Auth
  + Send messages to user by Id from JWT


## Testing
+ XUnit v3
+ AwesomeAssertions (FluentAssertion)
+ AwesomeAssertions.Json
  + Configure JToken parser
+ TestCase-Fixtures (experimental) - analog of [Pytest fixture with "scope=function"](https://docs.pytest.org/en/6.2.x/fixture.html#fixture-scopes)
  + XUnit integration
  + Fixture examples
+ Testing.WebApplicationFactory
  + Override app setup on test's 'ARRANGE'
  + FakeRandom
  + FakeTimeProvider
  + Testing-HttpClient
  + Testing-SignalR-Client
  + Random DB name prefix (for postgres) for each test
  + Random Key/Channel prefix (for redis) for each test
+ Example API tests
  + Authorized HttpClient/SignalRClient
+ Infrastructure API tests
+ "openapi.json" should not be changed
+ coverage
  + exclude test-projects
  + exclude GeneratedCode
  + make vscode to use ".runsettings"