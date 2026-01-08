namespace FEFF.Extensions;

public static class SystemExtensions
{
    public static Instant GetInstant(this TimeProvider timeProvider)
        => Instant.FromDateTimeOffset(timeProvider.GetUtcNow());
}