using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IBookingNotification
{
    Task SendCreatedAsync(
        BookingId bookingId,
        ResourceId resourceId,
        DateTime start,
        DateTime end);

    Task SendCancelledAsync(
        BookingId bookingId,
        ResourceId resourceId);
}
