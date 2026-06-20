namespace BookingDDD.Core.Application.Queries.GetBookingsForResource;

public sealed record BookingForResourceDto(
    Guid BookingId,
    Guid ResourceId,
    DateTime Start,
    DateTime End,
    string Status);
