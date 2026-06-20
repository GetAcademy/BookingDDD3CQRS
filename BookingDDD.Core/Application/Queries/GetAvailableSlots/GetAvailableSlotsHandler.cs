using BookingDDD.Core.Application.Queries;

namespace BookingDDD.Core.Application.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsHandler(IBookingQueries bookingQueries)
{
    public Task<IReadOnlyCollection<AvailableSlotDto>> HandleAsync(
        GetAvailableSlotsQuery query) =>
        bookingQueries.GetAvailableSlotsAsync(
            query.ResourceId,
            query.Date);
}
