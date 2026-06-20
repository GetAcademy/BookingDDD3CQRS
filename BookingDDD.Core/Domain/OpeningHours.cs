namespace BookingDDD.Core.Domain;

public sealed record OpeningHours
{
    public OpeningHours(int opensAtHour, int closesAtHour)
    {
        if (opensAtHour is < 0 or > 23)
        {
            throw new ArgumentOutOfRangeException(
                nameof(opensAtHour),
                "Opening hour must be between 0 and 23.");
        }

        if (closesAtHour is < 1 or > 23)
        {
            throw new ArgumentOutOfRangeException(
                nameof(closesAtHour),
                "Closing hour must be between 1 and 23.");
        }

        if (opensAtHour >= closesAtHour)
        {
            throw new ArgumentException("Opening hour must be before closing hour.");
        }

        OpensAtHour = opensAtHour;
        ClosesAtHour = closesAtHour;
    }

    public int OpensAtHour { get; }
    public int ClosesAtHour { get; }

    public bool Contains(BookingPeriod period) =>
        period.Start.Date == period.End.Date &&
        period.Start.Hour >= OpensAtHour &&
        period.End.Hour <= ClosesAtHour;
}
