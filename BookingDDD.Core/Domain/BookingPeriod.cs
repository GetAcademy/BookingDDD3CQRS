namespace BookingDDD.Core.Domain;

public sealed record BookingPeriod
{
    private BookingPeriod(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public DateTime Start { get; }
    public DateTime End { get; }

    public static Result<BookingPeriod> Create(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            return Result<BookingPeriod>.Fail("Start must be before end.");
        }

        if (!IsWholeHour(start) || !IsWholeHour(end))
        {
            return Result<BookingPeriod>.Fail("Only whole hours can be booked.");
        }

        return Result<BookingPeriod>.Success(new BookingPeriod(start, end));
    }

    public bool HasStarted(DateTime now) => Start <= now;

    public bool Overlaps(BookingPeriod other) =>
        other.Start < End && other.End > Start;

    public bool IsIn(OpeningHours openingHours) =>
        openingHours.Contains(this);

    private static bool IsWholeHour(DateTime value) =>
        value.Minute == 0 &&
        value.Second == 0 &&
        value.Millisecond == 0;
}
