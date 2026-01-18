using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Xunit.v3;

namespace FEFF.Extentions.Testing;

public sealed class SkipUnlessCIAttribute(string? msg = null) :
    BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        var envCI = SkipHelper.IsCIEnv();
        if(envCI == false)
            SkipHelper.ThrowSkipException(msg ?? "The test is skipped UNLESS runs at CI");
    }
}

public sealed class SkipWhenCIAttribute(string? msg = null) :
    BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        var envCI = SkipHelper.IsCIEnv();
        if(envCI == true)
            SkipHelper.ThrowSkipException(msg ?? "The test is skipped WHEN runs at CI");
    }
}

public static class SkipHelper
{
    public const string CiEnvVarName = "IS_CI_TEST";

    public static bool IsCIEnv() =>
        Environment.GetEnvironmentVariable(CiEnvVarName)?
        .ToLowerInvariant()
        == "true";

    [DoesNotReturn]
    public static void ThrowSkipException(string msg)
    {
        // We use the dynamic skip exception message pattern to turn this into a skipped test
        throw new Exception($"$XunitDynamicSkip${msg}");
    }
}