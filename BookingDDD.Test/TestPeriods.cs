using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

internal static class TestPeriods
{
    public static BookingPeriod Create(int startHour, int endHour) =>
        Create(2026, 6, 15, startHour, endHour);

    public static BookingPeriod Create(
        int year,
        int month,
        int day,
        int startHour,
        int endHour)
    {
        var result = BookingPeriod.Create(
            new DateTime(year, month, day, startHour, 0, 0),
            new DateTime(year, month, day, endHour, 0, 0));

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        return result.Value!;
    }
}
