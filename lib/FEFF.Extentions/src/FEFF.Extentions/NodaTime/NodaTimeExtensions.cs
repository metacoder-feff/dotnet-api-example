using NodaTime.Text;

namespace FEFF.Extentions;

public static class NodaTimeExtensions
{
    public static Instant GetInstant(this TimeProvider timeProvider)
        => Instant.FromDateTimeOffset(timeProvider.GetUtcNow());

    public static T ParseOrThrow<T>(this IPattern<T> src, string text)
    {
        return src.Parse(text).GetValueOrThrow();
    }
}