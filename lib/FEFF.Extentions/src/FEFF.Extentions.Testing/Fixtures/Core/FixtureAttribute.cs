namespace FEFF.Extentions.Fixtures;

[AttributeUsage(AttributeTargets.Class)]
public class FixtureAttribute : Attribute
{
    public Type? FixtureType { get; set; }
}
