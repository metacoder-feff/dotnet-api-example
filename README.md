# dotnet-api-example
## Features:
+ NodaTime
+ TimeProvider
+ Minimal Api: ConfigureJsonSerializer
  + snake_case
  + JsonStringEnumConverter
  + NodaTime integration
+ openapi/swagger-ui only in DEV + test
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
+ XUnit-3
+ AwesomeAssertions (FluentAssertion)
+ AwesomeAssertions.Json 
+ WebApplicationFactory
+ TestClient
+ FakeRandom
+ FakeTimeProvider
+ "openapi.json" should not be changed
+ coverage
  + exlude test-projects
  + exclude GeneratedCode
  + make vscode to use ".runsettings"


## TODO:
- logs-scopes (auth) + test
- auth/jwt + tests + swagger
- db      + tests + health
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
- better swagger + comments+noda,
- Noda+=openapi integration