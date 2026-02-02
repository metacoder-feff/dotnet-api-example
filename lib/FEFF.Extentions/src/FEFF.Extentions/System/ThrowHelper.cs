using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

[DebuggerNonUserCode]
public static class ThrowHelper
{
    /// <summary>
    /// Throws "InvalidOperationException".
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Assert(
        [DoesNotReturnIf(false)]
            bool argument,
        [CallerArgumentExpression(nameof(argument))]
            string? paramName = null)
    {
        if (argument == false)
        {
            throw new InvalidOperationException($"Assertion violated: '{paramName}'");
        }
    }
}