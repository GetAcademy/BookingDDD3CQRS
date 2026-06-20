using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Infrastructure;

public sealed class DapperResourceRepository : IResourceRepository
{
    private readonly DapperUnitOfWork _unitOfWork;

    public DapperResourceRepository(DapperUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Resource?> GetByIdAsync(ResourceId resourceId)
    {
        const string resourceSql = """
            SELECT Id, Name, OpensAtHour, ClosesAtHour
            FROM dbo.Resources WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @Id;
            """;

        const string bookingsSql = """
            SELECT Id, StartTime, EndTime, Status
            FROM dbo.Bookings
            WHERE ResourceId = @ResourceId
            ORDER BY StartTime;
            """;

        var resourceRow =
            await _unitOfWork.QuerySingleOrDefaultAsync<ResourceRow>(
                resourceSql,
                new { Id = resourceId.Value });

        if (resourceRow is null)
        {
            return null;
        }

        var bookingRows = await _unitOfWork.QueryAsync<BookingRow>(
            bookingsSql,
            new { ResourceId = resourceId.Value });

        var bookings = bookingRows.Select(ToDomain).ToArray();

        return Resource.Rehydrate(
            new ResourceId(resourceRow.Id),
            resourceRow.Name,
            new OpeningHours(
                resourceRow.OpensAtHour,
                resourceRow.ClosesAtHour),
            bookings);
    }

    public async Task SaveAsync(Resource resource)
    {
        const string updateResourceSql = """
            UPDATE dbo.Resources
            SET Name = @Name,
                OpensAtHour = @OpensAtHour,
                ClosesAtHour = @ClosesAtHour
            WHERE Id = @Id;
            """;

        const string saveBookingSql = """
            UPDATE dbo.Bookings
            SET StartTime = @StartTime,
                EndTime = @EndTime,
                Status = @Status
            WHERE Id = @Id
              AND ResourceId = @ResourceId;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.Bookings
                    (Id, ResourceId, StartTime, EndTime, Status)
                VALUES
                    (@Id, @ResourceId, @StartTime, @EndTime, @Status);
            END;
            """;

        await _unitOfWork.ExecuteAsync(
            updateResourceSql,
            new
            {
                Id = resource.Id.Value,
                resource.Name,
                resource.OpeningHours.OpensAtHour,
                resource.OpeningHours.ClosesAtHour
            });

        foreach (var booking in resource.Bookings)
        {
            await _unitOfWork.ExecuteAsync(
                saveBookingSql,
                new
                {
                    Id = booking.Id.Value,
                    ResourceId = resource.Id.Value,
                    StartTime = booking.Period.Start,
                    EndTime = booking.Period.End,
                    Status = (byte)booking.Status
                });
        }
    }

    private static Booking ToDomain(BookingRow row)
    {
        var periodResult = BookingPeriod.Create(row.StartTime, row.EndTime);
        if (periodResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Booking {row.Id} contains an invalid period: " +
                periodResult.ErrorMessage);
        }

        if (!Enum.IsDefined(typeof(BookingStatus), (int)row.Status))
        {
            throw new InvalidOperationException(
                $"Booking {row.Id} contains unknown status {row.Status}.");
        }

        return Booking.Rehydrate(
            new BookingId(row.Id),
            periodResult.Value!,
            (BookingStatus)row.Status);
    }

    private sealed class ResourceRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
        public int OpensAtHour { get; init; }
        public int ClosesAtHour { get; init; }
    }

    private sealed class BookingRow
    {
        public Guid Id { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public byte Status { get; init; }
    }
}
