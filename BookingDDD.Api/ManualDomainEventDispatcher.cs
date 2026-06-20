using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Application;
using BookingDDD.Core.Domain;

namespace BookingDDD.Api;

public sealed class ManualDomainEventDispatcher(
    AuditBookingCreatedHandler auditBookingCreated,
    AddBookingToCalendarHandler addBookingToCalendar,
    SendBookingConfirmationHandler sendBookingConfirmation,
    AuditBookingCancelledHandler auditBookingCancelled,
    RemoveBookingFromCalendarHandler removeBookingFromCalendar,
    SendBookingCancellationHandler sendBookingCancellation)
    : IDomainEventDispatcher
{
    public async Task PublishAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case BookingCreated bookingCreated:
                    await auditBookingCreated.HandleAsync(bookingCreated);
                    await addBookingToCalendar.HandleAsync(bookingCreated);
                    await sendBookingConfirmation.HandleAsync(bookingCreated);
                    break;

                case BookingCancelled bookingCancelled:
                    await auditBookingCancelled.HandleAsync(bookingCancelled);
                    await removeBookingFromCalendar.HandleAsync(bookingCancelled);
                    await sendBookingCancellation.HandleAsync(bookingCancelled);
                    break;

                default:
                    throw new NotSupportedException(
                        $"No handlers have been added manually for " +
                        $"{domainEvent.GetType().Name}.");
            }
        }
    }
}
