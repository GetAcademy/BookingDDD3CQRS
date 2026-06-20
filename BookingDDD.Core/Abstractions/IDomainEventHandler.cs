using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent);
}
