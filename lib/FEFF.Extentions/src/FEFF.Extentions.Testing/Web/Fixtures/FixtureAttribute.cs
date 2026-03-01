namespace FEFF.Extentions.Testing;

[AttributeUsage(AttributeTargets.Class)]
public class FixtureAttribute : Attribute
{
    public Type? FixtureType { get; }
}
