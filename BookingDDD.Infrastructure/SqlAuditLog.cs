using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookingDDD.Infrastructure;

public sealed class SqlAuditLog(SqlServerOptions options) : IAuditLog
{
    public async Task RecordAsync(
        string eventName,
        BookingId bookingId,
        ResourceId resourceId,
        DateTime occurredAtUtc)
    {
        const string sql = """
            INSERT INTO dbo.AuditLogEntries
                (Id, EventName, BookingId, ResourceId, OccurredAtUtc)
            VALUES
                (@Id, @EventName, @BookingId, @ResourceId, @OccurredAtUtc);
            """;

        await using var connection =
            new SqlConnection(options.ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = Guid.NewGuid(),
                EventName = eventName,
                BookingId = bookingId.Value,
                ResourceId = resourceId.Value,
                OccurredAtUtc = occurredAtUtc
            });
    }
}
