namespace FEFF.Extentions;

public static class MiscExtentions
{
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