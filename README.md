# dotnet-api-example
## Features:
+ Api-Modules
+ Example-Api-Module
+ NodaTime
+ TimeProvider
+ use openapi/swagger-ui only in DEV + test
+ use metrics + test
+ cloud-compatible logging (jsonl/stdout) + test
+ exceptions at 'app.Run()' are logged + test

## Testing
+ XUnit-3
+ AwesomeAssertions (FluentAssertion)
+ AwesomeAssertions.Json 
+ WebApplicationFactory
+ TestClient
+ FakeRandom
+ FakeTimeProvider
+ "openapi.json" should not be changed


## TODO:
- logs-scopes+test
- auth/jwt + tests + swagger
- db      + tests
- docker
- ci/github + new coverage + test results
- logging + tests
- health  + tests
- redis   + tests
- s3      + tests
- restructure
- make
- bg/periodic
- bg/offload
- fake-http-client stub (for tests)
- ...
- Object-diff / JSON-diff:
- try: https://github.com/weichch/system-text-json-jsondiffpatch
- try: https://github.com/nrkno/Quibble.Xunit
- try: https://github.com/awesomeassertions/awesomeassertions.json (patch this)
- ...
- better fake-random