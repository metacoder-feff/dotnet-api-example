using AwesomeAssertions.Numeric;
using NodaTime.Text;

namespace AwesomeAssertions;

//TODO: search github
public static class NodaTimeExtentions
{
    public static AndConstraint<ComparableTypeAssertions<Instant>> Be(this ComparableTypeAssertions<Instant> src, string isoTimeString)
    {
        var t = InstantPattern.ExtendedIso.ParseOrThrow(isoTimeString);
        return src.Be(t);
    }
}