using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application.Queries.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(
    ResourceId ResourceId,
    DateOnly Date);
