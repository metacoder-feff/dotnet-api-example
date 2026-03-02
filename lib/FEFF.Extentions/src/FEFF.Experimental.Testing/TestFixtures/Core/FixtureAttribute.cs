namespace FEFF.Experimental.TestFixtures;

[AttributeUsage(AttributeTargets.Class)]
public class FixtureAttribute : Attribute
{
    public Type? FixtureType { get; set; }
}
