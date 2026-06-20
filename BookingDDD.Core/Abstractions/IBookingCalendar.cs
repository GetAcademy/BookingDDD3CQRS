using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IBookingCalendar
{
    Task AddAsync(
        BookingId bookingId,
        ResourceId resourceId,
        DateTime start,
        DateTime end);

    Task RemoveAsync(BookingId bookingId);
}
