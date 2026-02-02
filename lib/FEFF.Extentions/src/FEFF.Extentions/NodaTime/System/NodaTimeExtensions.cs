namespace System;

public static class NodaTimeExtensions
{
    public static Instant GetInstant(this TimeProvider timeProvider)
        => Instant.FromDateTimeOffset(timeProvider.GetUtcNow());
}