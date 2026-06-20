using BookingDDD.Core.Application.Queries;
using BookingDDD.Core.Application.Queries.GetAvailableSlots;
using BookingDDD.Core.Application.Queries.GetBookingsForResource;
using BookingDDD.Core.Application.Queries.GetCalendarForDay;
using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

public class QueryHandlerTests
{
    [Test]
    public async Task GetBookingsForResourceHandler_ReturnsReadModelDtos()
    {
        var resourceId = ResourceId.New();
        var dto = new BookingForResourceDto(
            Guid.NewGuid(),
            resourceId.Value,
            new DateTime(2026, 6, 15, 10, 0, 0),
            new DateTime(2026, 6, 15, 11, 0, 0),
            "Active");
        var queries = new FakeBookingQueries { Bookings = [dto] };
        var handler = new GetBookingsForResourceHandler(queries);

        var result = await handler.HandleAsync(
            new GetBookingsForResourceQuery(resourceId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Single(), Is.EqualTo(dto));
            Assert.That(queries.GetBookingsForResourceCount,
                Is.EqualTo(1));
            Assert.That(queries.LastResourceId, Is.EqualTo(resourceId));
        });
    }

    private sealed class FakeBookingQueries : IBookingQueries
    {
        public IReadOnlyCollection<BookingForResourceDto> Bookings
        {
            get;
            init;
        } = Array.Empty<BookingForResourceDto>();

        public int GetBookingsForResourceCount { get; private set; }
        public ResourceId LastResourceId { get; private set; }

        public Task<IReadOnlyCollection<BookingForResourceDto>>
            GetBookingsForResourceAsync(ResourceId resourceId)
        {
            GetBookingsForResourceCount++;
            LastResourceId = resourceId;
            return Task.FromResult(Bookings);
        }

        public Task<IReadOnlyCollection<CalendarItemDto>>
            GetCalendarForDayAsync(DateOnly date) =>
            Task.FromResult<IReadOnlyCollection<CalendarItemDto>>(
                Array.Empty<CalendarItemDto>());

        public Task<IReadOnlyCollection<AvailableSlotDto>>
            GetAvailableSlotsAsync(ResourceId resourceId, DateOnly date) =>
            Task.FromResult<IReadOnlyCollection<AvailableSlotDto>>(
                Array.Empty<AvailableSlotDto>());
    }
}
