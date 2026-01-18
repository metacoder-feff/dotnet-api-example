using Microsoft.Extensions.Time.Testing;

namespace FEFF.Extentions.Testing;

public static class FakeTimeExtensions
{
    public static void AdvanceSeconds(this FakeTimeProvider src, double delta)
    {
        src.Advance(TimeSpan.FromSeconds(delta));
    }

    public static void AdvanceMinutes(this FakeTimeProvider src, double delta)
    {
        src.Advance(TimeSpan.FromMinutes(delta));
    }

    public static void AdvanceHours(this FakeTimeProvider src, double delta)
    {
        src.Advance(TimeSpan.FromHours(delta));
    }

    public static void AdvanceDays(this FakeTimeProvider src, double delta)
    {
        src.Advance(TimeSpan.FromDays(delta));
    }

    public static void Advance(this FakeTimeProvider src, string timespan)
    {
        var ts = TimeSpan.Parse(timespan);
        src.Advance(ts);
    }

    public static void SetNow(this FakeTimeProvider src, string isoTimeString)
    {
        var d = DateTimeOffset.Parse(isoTimeString);
        src.SetUtcNow(d.ToUniversalTime()); // FakeTime bug: does not reset timezone/offset
    }
    
    public static void SetNow(this FakeTimeProvider src, Instant now)
    {
        src.SetUtcNow(now.ToDateTimeOffset());
    }
}