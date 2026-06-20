namespace BookingDDD.Core.Application.Queries.GetAvailableSlots;

public sealed record AvailableSlotDto(
    DateTime Start,
    DateTime End);
