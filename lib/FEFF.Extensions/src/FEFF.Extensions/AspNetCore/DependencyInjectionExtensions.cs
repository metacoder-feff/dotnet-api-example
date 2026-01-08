using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;
using FEFF.Extensions.AspNetCore;

public static class AspNetCoreDependencyInjectionExtensions
{
    public static void AddHttpUserIdentityProvider(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddTransient<IUserIdentityProvider, HttpUserIdentityProvider>();
    }

    /// <summary>
    /// Add configuration from "appsettings.secrets.json" (parse right now)
    /// </summary>
    public static void AddAppSettingSecretsJson(this IConfigurationManager configuration)
    {
        var reloadOnChange = configuration.GetReloadConfigOnChangeValue();
        configuration
            .AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: reloadOnChange);
    }
}