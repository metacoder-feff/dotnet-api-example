using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FEFF.Extentions;

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
            string? argumentExpression = null)
    {
        if (argument == false)
        {
            throw new InvalidOperationException($"Assertion violated: '{argumentExpression}'");
        }
    }

    public static class Argument
    {
        public static void ThrowIfNullOrEmpty<T>(
            [NotNull]
                IEnumerable<T>? argument,
            [CallerArgumentExpression(nameof(argument))]
                string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);

            if(argument.Any() == false)
                throw new ArgumentException("The value cannot be an empty collection.", paramName);
        }
    }
}