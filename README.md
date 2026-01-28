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

## TODO:
- postgress: enums
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
  + HealthCheck (overview)
- s3      + tests
  + HealthCheck (overview)
- app layer - restructure
- make
- bg/periodic
- bg/offload
- fake-http-client stub (for tests) + CaseFixture
- better openapi + comments,
- SignalR vs SSE ? + tests + CaseFixture
  + auth
- DisableReloadConfigByDefault (exception: "The configured user limit (128) on the number of inotify...") - enable (+tests?)
- Test: CaseFixture
  + CreateDb/DeleteDb+ on/off
- RetryHelper
  + optimistic concurrency
- Extentions: test Guid-Concurrency - auto-update / add save interceptor
- postgress: master/slave
- Api-Modules
- (experiment) OpenTelemetry.Exporter.Prometheus.AspNetCore
- ...

admin:
-post_s3_url
-save_to_redis_list
-read_from_s3_in_bg

public:
-get_from_db