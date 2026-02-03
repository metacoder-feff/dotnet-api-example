namespace Microsoft.AspNetCore.Builder;

public static class ReloadConfigHelper
{
    public static bool GetReloadConfigOnChangeValue(this IConfiguration configuration)
    {
        const string reloadConfigOnChangeKey = "hostBuilder:reloadConfigOnChange";
        var result = true;
        if (configuration[reloadConfigOnChangeKey] is string reloadConfigOnChange)
        {
            if (!bool.TryParse(reloadConfigOnChange, out result))
            {
                throw new InvalidOperationException($"Failed to convert configuration value at '{configuration.GetSection(reloadConfigOnChangeKey).Path}' to type '{typeof(bool)}'.");
            }
        }
        return result;
    }

    /// <summary>
    /// On linux FileWatcher spends ulimit very fast.
    /// The exception generated:
    ///   "The configured user limit (128) on the number of inotify..."
    /// Otherwise you need extend ulimit at the host (container).
    /// The setting 'reloadConfigOnChange' is treated as enabled by default.
    /// This method will set this setting to false only if it is not defined yet.
    /// Env setting used to re-enable is: "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=true".
    /// </summary>
    public static void DisableReloadConfigByDefault()
    {
        // WebApplication.CreateBuilder uses constructor new WebApplicationBuilder(...)
        // which uses ConfigurationManager values to add config files to ConfigurationManager
        // At that moment ConfigurationManager is set only from Environment 
        // by: EnvironmentVariablesExtensions.AddEnvironmentVariables(this ..., string? prefix)

        var reloadConfigOnChangeKey = "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE";
        if(Environment.GetEnvironmentVariable(reloadConfigOnChangeKey) == null)
        {
            Environment.SetEnvironmentVariable(reloadConfigOnChangeKey, "false");
        }
    }
}