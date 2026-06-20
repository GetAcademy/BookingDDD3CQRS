using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

public class ResourceTests
{
    [Test]
    public void Book_AddsActiveBookingAndRegistersDomainEvent()
    {
        var resource = CreateResource();
        var period = TestPeriods.Create(10, 11);

        var result = resource.Book(period);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Status, Is.EqualTo(
                BookingStatus.Active));
            Assert.That(resource.Bookings, Has.Count.EqualTo(1));
            Assert.That(resource.DomainEvents.Single(),
                Is.TypeOf<BookingCreated>());
        });
    }

    [Test]
    public void Book_RejectsPeriodOutsideOpeningHours()
    {
        var resource = CreateResource();

        var result = resource.Book(TestPeriods.Create(18, 19));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.ErrorMessage, Is.EqualTo(
                "Booking must be within opening hours."));
            Assert.That(resource.Bookings, Is.Empty);
            Assert.That(resource.DomainEvents, Is.Empty);
        });
    }

    [Test]
    public void Book_RejectsOverlappingActiveBooking()
    {
        var resource = CreateResource();
        resource.Book(TestPeriods.Create(10, 12));
        resource.ClearDomainEvents();

        var result = resource.Book(TestPeriods.Create(11, 13));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.ErrorMessage, Is.EqualTo(
                "Resource is not available for this period."));
            Assert.That(resource.Bookings, Has.Count.EqualTo(1));
            Assert.That(resource.DomainEvents, Is.Empty);
        });
    }

    [Test]
    public void Book_AllowsOverlapWithCancelledBooking()
    {
        var cancelled = Booking.Rehydrate(
            BookingId.New(),
            TestPeriods.Create(10, 12),
            BookingStatus.Cancelled);
        var resource = CreateResource(cancelled);

        var result = resource.Book(TestPeriods.Create(11, 13));

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void CancelBooking_ChangesStateAndRegistersDomainEvent()
    {
        var resource = CreateResource();
        var booking = resource.Book(TestPeriods.Create(10, 11)).Value!;
        resource.ClearDomainEvents();

        var result = resource.CancelBooking(
            booking.Id,
            new DateTime(2026, 6, 15, 9, 0, 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(booking.Status, Is.EqualTo(
                BookingStatus.Cancelled));
            Assert.That(resource.DomainEvents.Single(),
                Is.TypeOf<BookingCancelled>());
        });
    }

    [Test]
    public void CancelBooking_RejectsCancellationAfterStart()
    {
        var resource = CreateResource();
        var booking = resource.Book(TestPeriods.Create(10, 11)).Value!;
        resource.ClearDomainEvents();

        var result = resource.CancelBooking(
            booking.Id,
            new DateTime(2026, 6, 15, 10, 0, 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active));
            Assert.That(resource.DomainEvents, Is.Empty);
        });
    }

    private static Resource CreateResource(params Booking[] bookings) =>
        Resource.Rehydrate(
            ResourceId.New(),
            "Meeting room",
            new OpeningHours(8, 16),
            bookings);
}
