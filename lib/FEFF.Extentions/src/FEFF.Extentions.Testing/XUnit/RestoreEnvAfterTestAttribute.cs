using System.Collections.Frozen;
using System.Reflection;
using Xunit.v3;

namespace FEFF.Extentions.Testing;

using Env = FrozenDictionary<string, string>;

//TODO: test
public sealed class RestoreProcessEnvironmentAfterTestAttribute :
    BeforeAfterTestAttribute
{
    private volatile Env? _oldEnv;

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        //Disallow parallel run
//TODO: interlocked transaction
//TODO: error??
        if (_oldEnv != null)
            throw new InvalidOperationException("Can't restore process environment in parallel tests. Consider using [Collection] attribute to all the test classes that will be part of a collection. Tests within the same collection run sequentially.");
            
        _oldEnv = EnvironmentHelper.GetEnvironmentVariables();
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
//TODO: interlocked transaction
        if(_oldEnv == null)
            return;

        var newEnv = EnvironmentHelper.GetEnvironmentVariables();

        RevertOldValues(_oldEnv, newEnv);
        RemoveNewValues(_oldEnv, newEnv);

        _oldEnv = null;
    }

    private static void RevertOldValues(Env oldEnv, Env newEnv)
    {
        foreach(var oldKvp in oldEnv)
        {
            string? newValue = newEnv.TryGetOrNull(oldKvp.Key);

            if (oldKvp.Value != newValue)
                Environment.SetEnvironmentVariable(oldKvp.Key, oldKvp.Value);
        }
    }

    private static void RemoveNewValues(Env oldEnv, Env newEnv)
    {
        foreach(var k in newEnv.Keys)
        {
            if(oldEnv.ContainsKey(k) == false)
                Environment.SetEnvironmentVariable(k, null);
        }
    }
}