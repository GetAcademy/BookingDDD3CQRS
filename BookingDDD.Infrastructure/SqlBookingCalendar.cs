using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookingDDD.Infrastructure;

public sealed class SqlBookingCalendar(SqlServerOptions options)
    : IBookingCalendar
{
    public async Task AddAsync(
        BookingId bookingId,
        ResourceId resourceId,
        DateTime start,
        DateTime end)
    {
        const string sql = """
            UPDATE dbo.CalendarEntries
            SET ResourceId = @ResourceId,
                StartTime = @StartTime,
                EndTime = @EndTime
            WHERE BookingId = @BookingId;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.CalendarEntries
                    (BookingId, ResourceId, StartTime, EndTime)
                VALUES
                    (@BookingId, @ResourceId, @StartTime, @EndTime);
            END;
            """;

        await ExecuteAsync(
            sql,
            new
            {
                BookingId = bookingId.Value,
                ResourceId = resourceId.Value,
                StartTime = start,
                EndTime = end
            });
    }

    public Task RemoveAsync(BookingId bookingId) =>
        ExecuteAsync(
            """
            DELETE FROM dbo.CalendarEntries
            WHERE BookingId = @BookingId;
            """,
            new { BookingId = bookingId.Value });

    private async Task ExecuteAsync(
        string sql,
        object parameters)
    {
        await using var connection =
            new SqlConnection(options.ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, parameters);
    }
}
