using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IDomainEventDispatcher
{
    Task PublishAsync(IReadOnlyCollection<IDomainEvent> domainEvents);
}
