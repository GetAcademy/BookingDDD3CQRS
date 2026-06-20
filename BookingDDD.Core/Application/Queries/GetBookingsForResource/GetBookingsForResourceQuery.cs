using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application.Queries.GetBookingsForResource;

public sealed record GetBookingsForResourceQuery(ResourceId ResourceId);
