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
+ prometheus metrics + test
+ cloud-compatible healthchecks + tests
  + liveness
  + readiness
  + overview
  + ConfigureJsonSerializer
+ cloud-compatible logging + tests
  + stdout
  + json-lines
  + timestamp format
+ exceptions at 'app.Run()' are logged + tests
+ appsettings.secrets.json + gitignore
+ Redis + tests
  + Singletone factory
  + Configure DI container
  + HealthCheck (overview)
+ SignalR + tests
  + ConfigureJsonSerializer
  + Redis (via singletone factory)


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