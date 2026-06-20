using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application;

public sealed class AuditBookingCreatedHandler(IAuditLog auditLog)
    : IDomainEventHandler<BookingCreated>
{
    public Task HandleAsync(BookingCreated domainEvent) =>
        auditLog.RecordAsync(
            nameof(BookingCreated),
            domainEvent.BookingId,
            domainEvent.ResourceId,
            DateTime.UtcNow);
}

public sealed class AddBookingToCalendarHandler(IBookingCalendar calendar)
    : IDomainEventHandler<BookingCreated>
{
    public Task HandleAsync(BookingCreated domainEvent) =>
        calendar.AddAsync(
            domainEvent.BookingId,
            domainEvent.ResourceId,
            domainEvent.Start,
            domainEvent.End);
}

public sealed class SendBookingConfirmationHandler(
    IBookingNotification notification)
    : IDomainEventHandler<BookingCreated>
{
    public Task HandleAsync(BookingCreated domainEvent) =>
        notification.SendCreatedAsync(
            domainEvent.BookingId,
            domainEvent.ResourceId,
            domainEvent.Start,
            domainEvent.End);
}
