using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Configuration;

public static class OptionsValidationExtensions
{
    // public static OptionsBuilder<TOptions> ValidateBy<TOptions, TValidator> (this OptionsBuilder<TOptions> builder)
    //     where TOptions : class
    //     where TValidator : class, IValidateOptions<TOptions>
    // {
    //     builder.Services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
    //     return builder;
    // }

    public static ValidationHelper<TOptions> ValidationHelper<TOptions>(this OptionsBuilder<TOptions> builder)
    where TOptions : class
    {
        return new ValidationHelper<TOptions>(builder);
    }
}

public class ValidationHelper<TOptions>(OptionsBuilder<TOptions> builder)
where TOptions : class
{
    /// <summary>
    /// Register Microsoft.Extensions.Options.IValidateOptions<TOptions> for validation.
    /// </summary>
    public OptionsBuilder<TOptions> ValidateBy<TValidator>()
    where TValidator : class, IValidateOptions<TOptions>
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
        return builder;
    }
}