# dotnet-api-example
## Features:
+ NodaTime
+ Minimal Api: ConfigureJsonSerializer
  + snake_case
  + JsonStringEnumConverter
  + NodaTime integration
+ DB: postgress + tests
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
+ cloud-compatible logging + tests
  + stdout
  + json-lines
  + timestamp format
+ exceptions at 'app.Run()' are logged + tests
+ appsettings.secrets.json + gitignore


## Testing
+ XUnit v3
+ AwesomeAssertions (FluentAssertion)
+ AwesomeAssertions.Json 
+ WebApplicationFactory
  + Override app setup
  + FakeRandom
  + FakeTimeProvider
+ Example API tests
+ Infrastructure API tests
+ "openapi.json" should not be changed
+ coverage
  + exclude test-projects
  + exclude GeneratedCode
  + make vscode to use ".runsettings"