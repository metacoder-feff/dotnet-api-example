namespace NodaTime.Text;

public static class NodaTimeTextExtensions
{
    public static T ParseOrThrow<T>(this IPattern<T> src, string text)
    {
        return src.Parse(text).GetValueOrThrow();
    }
}