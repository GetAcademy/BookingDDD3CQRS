using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

public class OpeningHoursTests
{
    [Test]
    public void Contains_ReturnsFalse_ForPeriodAcrossMultipleDays()
    {
        var openingHours = new OpeningHours(8, 16);
        var period = BookingPeriod.Create(
            new DateTime(2026, 6, 15, 10, 0, 0),
            new DateTime(2026, 6, 16, 11, 0, 0)).Value!;

        Assert.That(openingHours.Contains(period), Is.False);
    }

    [Test]
    public void Constructor_RejectsInvalidOpeningHours()
    {
        Assert.That(
            () => new OpeningHours(16, 8),
            Throws.ArgumentException);
    }
}
