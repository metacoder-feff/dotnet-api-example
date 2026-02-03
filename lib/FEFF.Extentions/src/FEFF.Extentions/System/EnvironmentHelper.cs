using System.Collections;
using System.Collections.Frozen;

//TODO: split namespaces
namespace FEFF.Extentions;

public static class EnvironmentHelper
{

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
    
    /// <summary>
    /// Typed version of Environment.GetEnvironmentVariables().
    /// Skips env records when key or value is null, that typically should not occur.
    /// </summary>
    public static FrozenDictionary<string, string> GetEnvironmentVariables()
    {
        return Environment
                    .GetEnvironmentVariables()
                    .Cast<DictionaryEntry>()
                    .Select(TryMakeTyped<string, string>)
                    .WhereNotNull()
                    .ToFrozenDictionary();
    }

    private static KeyValuePair<TKey, TVal>? TryMakeTyped<TKey, TVal>(DictionaryEntry src)
    {
        if(src.Key is TKey k == false)
            return null;

        if(src.Value is TVal v == false)
            return null;

        return new KeyValuePair<TKey, TVal>(k, v);
    }
}