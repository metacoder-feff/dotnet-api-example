using System.Diagnostics.CodeAnalysis;

namespace System;

/*
    NotNullWhen(false)] - is a PostCondition for C# analizer 
      meaning:
      an argument of a method is NotNull when the method returns true
*/

public static class SystemExtentions
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

//TODO: single method?
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
    where T : struct
    {
        // return enumerable.Where(e => e != null).Select(e => e!);
        foreach (var item in sequence)
        {
            if (item == null)
                continue;
            yield return item.Value;
        }
    }

//TODO: test
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
    where T : notnull
    {
        // return enumerable.Where(e => e != null).Select(e => e!);
        foreach (var item in sequence)
        {
            if (item == null)
                continue;
            yield return item;
        }
    }
    
//TODO: test
    // returns 'null' only when not found. Only allowed when 'TVal : notnull'.
    public static TVal? TryGetOrNull<TKey, TVal>(this IDictionary<TKey, TVal>src, TKey key)
    where TVal : notnull
    {
        var b = src.TryGetValue(key, out var value);
        if (b == false)
            return default;
        return value;
    }

//TODO: test
    // returns 'null' whether not found or found null
    public static TVal? TryGetBothNull<TKey, TVal>(this IDictionary<TKey, TVal?>src, TKey key)
    where TVal : notnull
    {
        var b = src.TryGetValue(key, out var value);
        if (b == false)
            return default;
        return value;
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? src)
    {
        return string.IsNullOrEmpty(src);
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IReadOnlyCollection<T>? src)
    {
        if (src == null)
            return true;

        return src.Count <= 0;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this T[]? src)
    {
        if (src == null)
            return true;

        return src.Length <= 0;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this List<T>? src)
    {
        if (src == null)
            return true;

        return src.Count <= 0;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T>? src)
    {
        if (src == null)
            return true;

        return src.Count <= 0;
    }

    public static IEnumerable<string> IterateLines(this TextReader tw)
    {
        string? line = tw.ReadLine();
        while (line != null)
        {
            yield return line;
            line = tw.ReadLine();
        }
    }
    public static string ToStringLower(this bool src)
    {
        //TODO: opimize??            
        return src.ToString().ToLowerInvariant();
    }
}