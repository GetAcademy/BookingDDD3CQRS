using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Infrastructure;

public sealed class ConsoleBookingNotification : IBookingNotification
{
    public Task SendCreatedAsync(
        BookingId bookingId,
        ResourceId resourceId,
        DateTime start,
        DateTime end)
    {
        Console.WriteLine(
            $"Booking confirmation: {bookingId.Value} for resource " +
            $"{resourceId.Value}, {start:O} - {end:O}");

        return Task.CompletedTask;
    }

    public Task SendCancelledAsync(
        BookingId bookingId,
        ResourceId resourceId)
    {
        Console.WriteLine(
            $"Booking cancellation: {bookingId.Value} for resource " +
            resourceId.Value);

        return Task.CompletedTask;
    }
}
