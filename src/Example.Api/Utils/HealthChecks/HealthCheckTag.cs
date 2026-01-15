namespace Utils.HealthChecks;

public static class HealthCheckTag 
{
    public const string Startup   = "HealthCheckType:Startup";
    public const string Readiness = "HealthCheckType:Readiness";
    public const string Liveness  = "HealthCheckType:Liveness";
}