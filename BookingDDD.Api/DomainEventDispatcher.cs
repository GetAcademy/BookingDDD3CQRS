using System.Collections;
using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Api;

public sealed class DomainEventDispatcher(IServiceProvider services)
    : IDomainEventDispatcher
{
    public async Task PublishAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>)
                .MakeGenericType(domainEvent.GetType());
            var handlersType = typeof(IEnumerable<>)
                .MakeGenericType(handlerType);
            var handlers = (IEnumerable)services.GetRequiredService(
                handlersType);
            var handleMethod = handlerType.GetMethod(
                nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

            foreach (var handler in handlers)
            {
                var task = (Task)handleMethod.Invoke(
                    handler,
                    [domainEvent])!;
                await task;
            }
        }
    }
}
