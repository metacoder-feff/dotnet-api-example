//TODO: split namespaces
namespace FEFF.Extentions.Web;

public static class ServiceCollectionExtentions
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
}