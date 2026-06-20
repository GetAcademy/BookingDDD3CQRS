namespace BookingDDD.Core.Domain;

public sealed class Booking
{
    private Booking(
        BookingId id,
        BookingPeriod period,
        BookingStatus status)
    {
        Id = id;
        Period = period;
        Status = status;
    }

    public BookingId Id { get; }
    public BookingPeriod Period { get; }
    public BookingStatus Status { get; private set; }
    public bool IsActive => Status == BookingStatus.Active;

    internal static Booking Create(BookingPeriod period) =>
        new(BookingId.New(), period, BookingStatus.Active);

    public static Booking Rehydrate(
        BookingId id,
        BookingPeriod period,
        BookingStatus status) =>
        new(id, period, status);

    internal Result<Booking> Cancel(DateTime now)
    {
        if (!IsActive)
        {
            return Result<Booking>.Fail("Booking is already cancelled.");
        }

        if (Period.HasStarted(now))
        {
            return Result<Booking>.Fail(
                "Cannot cancel booking after it has started.");
        }

        Status = BookingStatus.Cancelled;
        return Result<Booking>.Success(this);
    }

    internal bool Overlaps(BookingPeriod period) =>
        Period.Overlaps(period);
}
