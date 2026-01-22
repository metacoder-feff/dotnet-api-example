using System.Collections.Frozen;
using System.Reflection;
using Xunit.v3;

namespace FEFF.Extentions.Testing;

using Env = FrozenDictionary<string, string>;

//TODO: test
/// <summary>
/// Reverts ProcessEnvironment changes After Test.
/// Those tests should not be run in parallel otherwise the exception would be thrown.
/// Consider using [Collection] attribute to all the test classes that will be part of a collection.
/// Tests within the same collection run sequentially.
/// </summary>
public sealed class RestoreProcessEnvironmentAfterTestAttribute :
    BeforeAfterTestAttribute
{
    private readonly static object __lockObj = new();

    //Disallow parallel env saving or restoring Process-wide
    private static volatile Env? __oldEnv;
    
    // revert only allowed for the 'Case' the env was saved at.
    private volatile IXunitTestCase? _case;

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        lock(__lockObj)
        {
//TODO: correct error??
            if (__oldEnv != null)
                throw new InvalidOperationException("Can't restore process environment in parallel tests. Consider using [Collection] attribute to all the test classes that will be part of a collection. Tests within the same collection run sequentially.");
                
            __oldEnv = EnvironmentHelper.GetEnvironmentVariables();
            _case = test.TestCase;
        }
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        lock(__lockObj)
        {
            if(__oldEnv == null)
                return;

            if(_case != test.TestCase)
                return;

            var newEnv = EnvironmentHelper.GetEnvironmentVariables();

            RevertOldValues(__oldEnv, newEnv);
            RemoveNewValues(__oldEnv, newEnv);

            __oldEnv = null;
            _case = null;
        }
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