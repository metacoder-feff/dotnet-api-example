namespace System;

public static class SystemExtensions
{
    public static Instant GetInstant(this TimeProvider timeProvider)
        => Instant.FromDateTimeOffset(timeProvider.GetUtcNow());
}