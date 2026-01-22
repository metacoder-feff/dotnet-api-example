using System.Collections.Frozen;
using System.Reflection;

using Xunit.v3;

namespace FEFF.Extentions.Testing;

//TODO: test
public sealed class RestoreProcessEnvironmentAfterTestAttribute() :
    BeforeAfterTestAttribute
{
    private FrozenDictionary<string, string>? _oldEnv;

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        if (_oldEnv != null)
            return;
        _oldEnv = EnvironmentHelper.GetEnvironmentVariables();
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        if(_oldEnv == null)
            return;

        var newEnv = EnvironmentHelper.GetEnvironmentVariables();

        RevertOldValues(_oldEnv, newEnv);
        RemoveNewValues(_oldEnv, newEnv);
    }

    private static void RevertOldValues(FrozenDictionary<string, string> oldEnv, FrozenDictionary<string, string> newEnv)
    {
        foreach(var oldKvp in oldEnv)
        {
            string? newValue = newEnv.TryGetNotNull(oldKvp.Key);

            if (oldKvp.Value != newValue)
                Environment.SetEnvironmentVariable(oldKvp.Key, oldKvp.Value);
        }
    }

    private static void RemoveNewValues(FrozenDictionary<string, string> oldEnv, FrozenDictionary<string, string> newEnv)
    {
        foreach(var k in newEnv.Keys)
        {
            if(oldEnv.ContainsKey(k) == false)
                Environment.SetEnvironmentVariable(k, null);
        }
    }
}