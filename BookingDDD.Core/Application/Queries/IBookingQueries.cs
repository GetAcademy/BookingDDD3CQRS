using BookingDDD.Core.Domain;
using BookingDDD.Core.Application.Queries.GetAvailableSlots;
using BookingDDD.Core.Application.Queries.GetBookingsForResource;
using BookingDDD.Core.Application.Queries.GetCalendarForDay;

namespace BookingDDD.Core.Application.Queries;

public interface IBookingQueries
{
    Task<IReadOnlyCollection<BookingForResourceDto>>
        GetBookingsForResourceAsync(ResourceId resourceId);

    Task<IReadOnlyCollection<CalendarItemDto>>
        GetCalendarForDayAsync(DateOnly date);

    Task<IReadOnlyCollection<AvailableSlotDto>>
        GetAvailableSlotsAsync(ResourceId resourceId, DateOnly date);
}
