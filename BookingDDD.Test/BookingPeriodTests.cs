using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

public class BookingPeriodTests
{
    [Test]
    public void Create_ReturnsFailure_WhenStartIsNotBeforeEnd()
    {
        var start = new DateTime(2026, 6, 15, 10, 0, 0);

        var result = BookingPeriod.Create(start, start);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.ErrorMessage, Is.EqualTo(
            "Start must be before end."));
    }

    [Test]
    public void Create_ReturnsFailure_WhenPeriodDoesNotUseWholeHours()
    {
        var result = BookingPeriod.Create(
            new DateTime(2026, 6, 15, 10, 30, 0),
            new DateTime(2026, 6, 15, 11, 0, 0));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.ErrorMessage, Is.EqualTo(
            "Only whole hours can be booked."));
    }

    [Test]
    public void Equality_UsesStartAndEnd()
    {
        var first = TestPeriods.Create(10, 11);
        var second = TestPeriods.Create(10, 11);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Overlaps_ReturnsTrue_WhenPeriodsOverlap()
    {
        var first = TestPeriods.Create(10, 12);
        var second = TestPeriods.Create(11, 13);

        Assert.That(first.Overlaps(second), Is.True);
    }

    [Test]
    public void Overlaps_ReturnsFalse_WhenPeriodsAreAdjacent()
    {
        var first = TestPeriods.Create(10, 11);
        var second = TestPeriods.Create(11, 12);

        Assert.That(first.Overlaps(second), Is.False);
    }
}
