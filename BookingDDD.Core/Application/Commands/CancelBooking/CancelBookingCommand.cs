using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application.Commands.CancelBooking;

public sealed record CancelBookingCommand(
    BookingId BookingId,
    DateTime Now);
