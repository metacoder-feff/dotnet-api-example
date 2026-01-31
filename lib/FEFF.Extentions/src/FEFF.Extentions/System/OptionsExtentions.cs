using Microsoft.Extensions.Options;

namespace FEFF.Extentions;

public static class OptionsProviderExtentions
{
    public static TOptions GetNamedOrDeafault<TOptions>(this IOptionsFactory<TOptions> src, string? name = null)
        where TOptions : class
    {
        var k = name ?? Options.DefaultName;
        return src.Create(k);
    }
}

public static class OptionsBuilderExtentions
{
    public static ValidationHelper<TOptions> Validate<TOptions>(this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        return new ValidationHelper<TOptions>(builder);
    }
}

public class ValidationHelper<TOptions>
    where TOptions : class
{
    private readonly OptionsBuilder<TOptions> _builder;

    public ValidationHelper(OptionsBuilder<TOptions> builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// use Microsoft.Extensions.Options.IValidateOptions<TOptions>
    /// </summary>
    public OptionsBuilder<TOptions> With<TValidator>()
        where TValidator : class, IValidateOptions<TOptions>
    {
        _builder.Services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
        return _builder;
    }
}