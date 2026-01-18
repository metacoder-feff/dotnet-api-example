# dotnet-api-example
## Features:
+ NodaTime
+ TimeProvider
+ Minimal Api: ConfigureJsonSerializer
  + snake_case
  + JsonStringEnumConverter
  + NodaTime integration
+ openapi/swagger-ui only in DEV + test
  + NodaTime integration
+ metrics + test
+ cloud-compatible healthchecks + tests
  + liveness
  + readiness
  + overal
+ cloud-compatible logging + test
  + stdout
  + json-lines
  + timestamp format
+ exceptions at 'app.Run()' are logged + test

+ Api-Modules
+ Example-Api-Module

## Testing
+ XUnit v3
+ AwesomeAssertions (FluentAssertion)
+ AwesomeAssertions.Json 
+ WebApplicationFactory
+ TestClient
+ FakeRandom
+ FakeTimeProvider
+ "openapi.json" should not be changed
+ coverage
  + exclude test-projects
  + exclude GeneratedCode
  + make vscode to use ".runsettings"


## TODO:
- better openapi + comments,
- options += fluentvalidation
- test migrations
- test db optimizations
- logs-scopes example (auth) + test
- auth/jwt + tests + swagger
- db      + tests + health
- postgress+=node
- migration-test
- optimistic concurrency
- docker
- ci/github + new coverage + test results + bages
- redis   + tests  + health
- s3      + tests  + health
- restructure
- make
- bg/periodic
- bg/offload
- fake-http-client stub (for tests)
- options validation - error message
- ...