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
+ openapi/swagger-ui only in DEV + tests
  + NodaTime integration
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
+ appsettings.secrets.json + gitignore
+ Redis + tests
  + BL example
  + DI container: Singleton 'RedisConnectionManager'
  + DI container: ConfigureOptions using ConnectionString + delegate
  + RedisConnection HealthCheck (overview)
+ SignalR + tests
  + ConfigureJsonSerializer
  + Redis (via DI container)
  + RedisConnection HealthCheck (overview)


## Testing
+ XUnit v3
+ AwesomeAssertions (FluentAssertion)
+ AwesomeAssertions.Json
  + Configure JToken parser
+ CaseFixtures (experimental)
+ WebApplicationFactory
  + Override app setup
  + FakeRandom
  + FakeTimeProvider
  + SignalR-Client
  + random test DB name (for postgres)
  + random Key/Channel prefix (for redis)
+ Example API tests
+ Infrastructure API tests
+ "openapi.json" should not be changed
+ coverage
  + exclude test-projects
  + exclude GeneratedCode
  + make vscode to use ".runsettings"