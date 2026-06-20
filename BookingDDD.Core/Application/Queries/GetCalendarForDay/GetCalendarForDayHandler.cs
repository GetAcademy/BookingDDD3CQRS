using BookingDDD.Core.Application.Queries;

namespace BookingDDD.Core.Application.Queries.GetCalendarForDay;

public sealed class GetCalendarForDayHandler(IBookingQueries bookingQueries)
{
    public Task<IReadOnlyCollection<CalendarItemDto>> HandleAsync(
        GetCalendarForDayQuery query) =>
        bookingQueries.GetCalendarForDayAsync(query.Date);
}
