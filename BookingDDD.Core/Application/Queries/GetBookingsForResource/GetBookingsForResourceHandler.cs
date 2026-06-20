using BookingDDD.Core.Application.Queries;

namespace BookingDDD.Core.Application.Queries.GetBookingsForResource;

public sealed class GetBookingsForResourceHandler(
    IBookingQueries bookingQueries)
{
    public Task<IReadOnlyCollection<BookingForResourceDto>> HandleAsync(
        GetBookingsForResourceQuery query) =>
        bookingQueries.GetBookingsForResourceAsync(query.ResourceId);
}
