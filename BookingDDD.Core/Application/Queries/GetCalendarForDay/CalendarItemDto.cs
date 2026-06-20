namespace BookingDDD.Core.Application.Queries.GetCalendarForDay;

public sealed record CalendarItemDto(
    Guid BookingId,
    Guid ResourceId,
    string ResourceName,
    DateTime Start,
    DateTime End,
    string Status);
