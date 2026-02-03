using System.Diagnostics.CodeAnalysis;

namespace FEFF.Extentions;

public static class StringExtentions
{
    //TODO: tests
    public static string[] SplitLines(this string src)
    {
        return src.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool EndsWithOneOf(this string src, IEnumerable<string> ending, StringComparison sc)
    {
        foreach (var e in ending)
        {
            var b = src.EndsWith(e, sc);
            if (b == true)
                return true;
        }
        return false;
    }

    public static string RemoveEndingCaseInsensitive(this string src, string ending)
    {
        if (src.ToUpperInvariant().EndsWith(ending.ToUpperInvariant()))
        {
            var resLen = src.Length - ending.Length;
            if (resLen > 0)
                src = src.Substring(0, resLen);
        }
        return src;
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? src)
    {
        return string.IsNullOrEmpty(src);
    }
}