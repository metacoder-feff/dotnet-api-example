using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace FEFF.Experimental.TestFixtures;

internal sealed class Guard : IDisposable
{
//TODO: more unique
    private const string Key = $"{nameof(TestCaseFixturesXUnitExtensionAttribute)}.{nameof(Guard)}";

    public Guard()
    {
        TestContext.Current.KeyValueStorage[Key] = null;
    }

    public void Dispose()
    {
        TestContext.Current.KeyValueStorage.TryRemove(Key, out _);
    }

    public static bool IsInitialized()
    {
        return TestContext.Current.KeyValueStorage.ContainsKey(Key);
    }
}

/// <summary>
/// Manages <see cref="FixtureContainer"/> for each XUnit test. <br/>
/// Allows to use <see cref="TestContextExtentions.GetTestCaseFixtureProvider"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class TestCaseFixturesXUnitExtensionAttribute : BeforeAfterTestAttribute, IAssemblyFixtureAttribute
{
    public Type AssemblyFixtureType => typeof(Guard);

    // Before is called after TestClass ctor
    // => use IAssemblyFixtureAttribute
    // public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    // {
    // }

//TODO: async
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        var c = RemoveFixtureContainer(TestContext.Current, test);
        c?.DisposeAsync().AsTask().Wait();
    }

    //------------------

//TODO: more unique
    private static string GetKey(ITestMetadata test)
    {
        return $"{nameof(FixtureContainer)}-{test.UniqueID}";
    }

    private static FixtureContainer? RemoveFixtureContainer(ITestContext ctx, IXunitTest test)
    {
        var k = GetKey(test);

        ctx.KeyValueStorage.TryRemove(k, out var res);

        return res as FixtureContainer;
    }

    internal static FixtureContainer GetTestCaseFixtureProvider(ITestContext ctx)
    {
//TODO: Test TestEngineStatus.Initializing
//TODO: Argument
        ThrowHelper.Assert(ctx.TestStatus == TestEngineStatus.Initializing || ctx.TestStatus == TestEngineStatus.Running);
        //ThrowHelper.Assert(ctx.TestStatus == TestEngineStatus.Running);
        var test = ctx.Test;
        ThrowHelper.Assert(test != null);

//TODO: test message
        if(Guard.IsInitialized() == false)
           throw new InvalidOperationException($"Must use '{nameof(FEFF.Experimental.TestFixtures.TestCaseFixturesXUnitExtensionAttribute)}' a test assembly before calling '{nameof(GetTestCaseFixtureProvider)}'.");

        var k = GetKey(test);

        var obj = ctx.KeyValueStorage.GetOrAdd(k, (_) => new FixtureContainer());

//TODO: utils Cast()
        if(obj is not FixtureContainer res)
            throw new InvalidOperationException($"Stored object is not a {nameof(FixtureContainer)}");

        return res;
    }
}
