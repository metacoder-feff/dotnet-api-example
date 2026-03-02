namespace Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration configuration, string connectionStringName)
    {
        return configuration
            .GetConnectionString(connectionStringName) 
            ?? throw new InvalidOperationException($"ConnectionString not found: '{connectionStringName}'");
    }
}