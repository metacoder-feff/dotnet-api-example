# dotnet-api-example
## Features:
+ NodaTime
+ Minimal Api: ConfigureJsonSerializer
  + snake_case
  + JsonStringEnumConverter
  + NodaTime integration
+ openapi/swagger-ui only in DEV + tests
  + NodaTime integration
+ metrics + test
+ cloud-compatible healthchecks + tests
  + liveness
  + readiness (db)
  + overal
+ cloud-compatible logging + tests
  + stdout
  + json-lines
  + timestamp format
+ exceptions at 'app.Run()' are logged + tests

+ Api-Modules
+ Example-Api-Module

## Testing
+ XUnit v3
+ AwesomeAssertions (FluentAssertion)
+ AwesomeAssertions.Json 
+ WebApplicationFactory
  + Override app setup
+ Example API tests
+ Infrastructure API tests
+ FakeRandom
+ FakeTimeProvider
+ "openapi.json" should not be changed
+ coverage
  + exclude test-projects
  + exclude GeneratedCode
  + make vscode to use ".runsettings"


## TODO:
- secrets.json
- healthcheck overal - rename
- postgress + tests
  + NodaTime integration
  + HealthCheck (readiness)
  + optimistic concurrency
  + db updated at
+ AwesomeAssertions.Json - Parse options
- options += fluentvalidation
- options validation - error message
- test db migrations
- test db optimizations
- logs-scopes example (auth) + test
- auth/jwt + tests + swagger
- docker: build image
- ci/github + new coverage + test results + bages
- redis   + tests
  + HealthCheck (overal)
- s3      + tests
  + HealthCheck (overal)
- restructure
- make
- bg/periodic
- bg/offload
- fake-http-client stub (for tests)
- better openapi + comments,
- SignalR vs SSE ?
  + auth
- DisableReloadConfigByDefault (exception: "The configured user limit (128) on the number of inotify...") - enable + tests
- ...