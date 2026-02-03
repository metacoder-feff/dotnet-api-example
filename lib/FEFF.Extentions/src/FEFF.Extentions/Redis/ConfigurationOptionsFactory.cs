using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public class ConfigurationOptionsFactory : OptionsFactory<ConfigurationOptions>
{
    private readonly Func<ConfigurationOptions> _factory;

    public ConfigurationOptionsFactory(
        IEnumerable<IConfigureOptions<ConfigurationOptions>> setups, 
        IEnumerable<IPostConfigureOptions<ConfigurationOptions>> postConfigures,
        IOptions<Options> factoryOpts) 
    : base(setups, postConfigures)
    {
        _factory = factoryOpts.Value.Factory;
    }

    protected override ConfigurationOptions CreateInstance(string name)
    {
        return _factory(); // e.g. ConfigurationOptions.Parse(...)
        //return base.CreateInstance(name);
    }

    public class Options
    {
        public Func<ConfigurationOptions> Factory { get; set; } = DefaultFactory;
        
        public static ConfigurationOptions DefaultFactory() => new();
    }
}