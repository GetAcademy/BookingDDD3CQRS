using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application;

public sealed class AuditBookingCancelledHandler(IAuditLog auditLog)
    : IDomainEventHandler<BookingCancelled>
{
    public Task HandleAsync(BookingCancelled domainEvent) =>
        auditLog.RecordAsync(
            nameof(BookingCancelled),
            domainEvent.BookingId,
            domainEvent.ResourceId,
            DateTime.UtcNow);
}

public sealed class RemoveBookingFromCalendarHandler(IBookingCalendar calendar)
    : IDomainEventHandler<BookingCancelled>
{
    public Task HandleAsync(BookingCancelled domainEvent) =>
        calendar.RemoveAsync(domainEvent.BookingId);
}

public sealed class SendBookingCancellationHandler(
    IBookingNotification notification)
    : IDomainEventHandler<BookingCancelled>
{
    public Task HandleAsync(BookingCancelled domainEvent) =>
        notification.SendCancelledAsync(
            domainEvent.BookingId,
            domainEvent.ResourceId);
}
