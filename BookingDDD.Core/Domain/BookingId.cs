namespace BookingDDD.Core.Domain;

public readonly record struct BookingId(Guid Value)
{
    public static BookingId New() => new(Guid.NewGuid());
}
