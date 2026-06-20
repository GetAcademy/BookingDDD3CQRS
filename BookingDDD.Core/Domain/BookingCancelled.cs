namespace BookingDDD.Core.Domain;

public sealed record BookingCancelled(
    BookingId BookingId,
    ResourceId ResourceId,
    DateTime Start,
    DateTime End) : IDomainEvent;
