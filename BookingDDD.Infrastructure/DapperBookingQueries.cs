using BookingDDD.Core.Application.Queries;
using BookingDDD.Core.Application.Queries.GetAvailableSlots;
using BookingDDD.Core.Application.Queries.GetBookingsForResource;
using BookingDDD.Core.Application.Queries.GetCalendarForDay;
using BookingDDD.Core.Domain;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookingDDD.Infrastructure;

public sealed class DapperBookingQueries : IBookingQueries
{
    private readonly SqlServerOptions _options;

    public DapperBookingQueries(SqlServerOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyCollection<BookingForResourceDto>>
        GetBookingsForResourceAsync(ResourceId resourceId)
    {
        const string sql = """
            SELECT
                Id AS BookingId,
                ResourceId,
                StartTime AS [Start],
                EndTime AS [End],
                Status
            FROM dbo.Bookings
            WHERE ResourceId = @ResourceId
            ORDER BY StartTime;
            """;

        await using var connection =
            new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<BookingForResourceRow>(
            sql,
            new { ResourceId = resourceId.Value });

        return rows
            .Select(row => new BookingForResourceDto(
                row.BookingId,
                row.ResourceId,
                row.Start,
                row.End,
                ToStatusName(row.Status)))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<CalendarItemDto>>
        GetCalendarForDayAsync(DateOnly date)
    {
        const string sql = """
            SELECT
                b.Id AS BookingId,
                b.ResourceId,
                r.Name AS ResourceName,
                b.StartTime AS [Start],
                b.EndTime AS [End],
                b.Status
            FROM dbo.Bookings b
            INNER JOIN dbo.Resources r ON r.Id = b.ResourceId
            WHERE b.StartTime < @EndOfDay
              AND b.EndTime > @StartOfDay
            ORDER BY b.StartTime, r.Name;
            """;

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = startOfDay.AddDays(1);

        await using var connection =
            new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<CalendarItemRow>(
            sql,
            new { StartOfDay = startOfDay, EndOfDay = endOfDay });

        return rows
            .Select(row => new CalendarItemDto(
                row.BookingId,
                row.ResourceId,
                row.ResourceName,
                row.Start,
                row.End,
                ToStatusName(row.Status)))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<AvailableSlotDto>>
        GetAvailableSlotsAsync(ResourceId resourceId, DateOnly date)
    {
        const string resourceSql = """
            SELECT OpensAtHour, ClosesAtHour
            FROM dbo.Resources
            WHERE Id = @ResourceId;
            """;

        const string bookingsSql = """
            SELECT
                StartTime AS [Start],
                EndTime AS [End]
            FROM dbo.Bookings
            WHERE ResourceId = @ResourceId
              AND Status = @ActiveStatus
              AND StartTime < @EndOfDay
              AND EndTime > @StartOfDay
            ORDER BY StartTime;
            """;

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = startOfDay.AddDays(1);

        await using var connection =
            new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        var openingHours =
            await connection.QuerySingleOrDefaultAsync<OpeningHoursRow>(
                resourceSql,
                new { ResourceId = resourceId.Value });

        if (openingHours is null)
        {
            return Array.Empty<AvailableSlotDto>();
        }

        var bookings = (await connection.QueryAsync<BookingPeriodRow>(
            bookingsSql,
            new
            {
                ResourceId = resourceId.Value,
                ActiveStatus = (byte)BookingStatus.Active,
                StartOfDay = startOfDay,
                EndOfDay = endOfDay
            })).AsList();

        var opening = date.ToDateTime(
            new TimeOnly(openingHours.OpensAtHour, 0));
        var closing = date.ToDateTime(
            new TimeOnly(openingHours.ClosesAtHour, 0));
        var slots = new List<AvailableSlotDto>();

        for (var start = opening; start < closing; start = start.AddHours(1))
        {
            var end = start.AddHours(1);
            var overlapsBooking = bookings.Any(booking =>
                booking.Start < end && booking.End > start);

            if (!overlapsBooking)
            {
                slots.Add(new AvailableSlotDto(start, end));
            }
        }

        return slots;
    }

    private static string ToStatusName(byte status) =>
        Enum.IsDefined(typeof(BookingStatus), (int)status)
            ? ((BookingStatus)status).ToString()
            : $"Unknown ({status})";

    private sealed class BookingForResourceRow
    {
        public Guid BookingId { get; init; }
        public Guid ResourceId { get; init; }
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public byte Status { get; init; }
    }

    private sealed class CalendarItemRow
    {
        public Guid BookingId { get; init; }
        public Guid ResourceId { get; init; }
        public string ResourceName { get; init; } = "";
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public byte Status { get; init; }
    }

    private sealed class OpeningHoursRow
    {
        public int OpensAtHour { get; init; }
        public int ClosesAtHour { get; init; }
    }

    private sealed class BookingPeriodRow
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
    }
}
