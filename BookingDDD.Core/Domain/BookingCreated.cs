namespace BookingDDD.Core.Domain;

public sealed record BookingCreated(
    BookingId BookingId,
    ResourceId ResourceId,
    DateTime Start,
    DateTime End) : IDomainEvent;
