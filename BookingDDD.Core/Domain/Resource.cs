namespace BookingDDD.Core.Domain;

public sealed class Resource : AggregateRoot
{
    private readonly List<Booking> _bookings;

    private Resource(
        ResourceId id,
        string name,
        OpeningHours openingHours,
        IEnumerable<Booking> bookings)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Resource name is required.", nameof(name));
        }

        Id = id;
        Name = name;
        OpeningHours = openingHours;
        _bookings = bookings.ToList();
    }

    public ResourceId Id { get; }
    public string Name { get; }
    public OpeningHours OpeningHours { get; }
    public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();

    public static Resource Create(
        ResourceId id,
        string name,
        OpeningHours openingHours) =>
        new(id, name, openingHours, []);

    public static Resource Rehydrate(
        ResourceId id,
        string name,
        OpeningHours openingHours,
        IEnumerable<Booking> bookings) =>
        new(id, name, openingHours, bookings);

    public Result<Booking> Book(BookingPeriod period)
    {
        if (!period.IsIn(OpeningHours))
        {
            return Result<Booking>.Fail(
                "Booking must be within opening hours.");
        }

        if (_bookings.Any(booking =>
                booking.IsActive && booking.Overlaps(period)))
        {
            return Result<Booking>.Fail(
                "Resource is not available for this period.");
        }

        var booking = Booking.Create(period);
        _bookings.Add(booking);

        AddDomainEvent(new BookingCreated(
            booking.Id,
            Id,
            period.Start,
            period.End));

        return Result<Booking>.Success(booking);
    }

    public Result<Booking> CancelBooking(BookingId bookingId, DateTime now)
    {
        var booking = _bookings.SingleOrDefault(candidate =>
            candidate.Id == bookingId);

        if (booking is null)
        {
            return Result<Booking>.Fail(
                "Booking does not exist on this resource.");
        }

        var result = booking.Cancel(now);
        if (result.IsFailure)
        {
            return result;
        }

        AddDomainEvent(new BookingCancelled(
            booking.Id,
            Id,
            booking.Period.Start,
            booking.Period.End));

        return result;
    }
}
