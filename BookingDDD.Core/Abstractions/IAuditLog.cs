using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IAuditLog
{
    Task RecordAsync(
        string eventName,
        BookingId bookingId,
        ResourceId resourceId,
        DateTime occurredAtUtc);
}
