using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application.Commands.BookResource;

public sealed record BookResourceCommand(
    ResourceId ResourceId,
    DateTime Start,
    DateTime End);
