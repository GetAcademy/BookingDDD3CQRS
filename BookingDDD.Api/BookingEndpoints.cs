using BookingDDD.Core.Application.Commands.BookResource;
using BookingDDD.Core.Application.Commands.CancelBooking;
using BookingDDD.Core.Application.Queries.GetAvailableSlots;
using BookingDDD.Core.Application.Queries.GetBookingsForResource;
using BookingDDD.Core.Application.Queries.GetCalendarForDay;
using BookingDDD.Core.Domain;

namespace BookingDDD.Api;

public static class BookingEndpoints
{
    public static async Task<IResult> CreateBookingAsync(
        Guid resourceId,
        CreateBookingRequest request,
        BookResourceHandler handler)
    {
        var command = new BookResourceCommand(
            new ResourceId(resourceId),
            request.Start,
            request.End);
        var result = await handler.HandleAsync(command);

        if (result.IsFailure)
        {
            return ToFailureHttpResult(result.ErrorMessage!);
        }

        var response = BookingResponse.From(result.Value!);

        return Results.Created(
            $"/api/resources/{resourceId}/bookings/{response.Id}",
            response);
    }

    public static async Task<IResult> CancelBookingAsync(
        Guid bookingId,
        CancelBookingHandler handler,
        TimeProvider timeProvider)
    {
        var command = new CancelBookingCommand(
            new BookingId(bookingId),
            timeProvider.GetLocalNow().DateTime);
        var result = await handler.HandleAsync(command);

        return result.IsSuccess
            ? Results.Ok(BookingResponse.From(result.Value!))
            : ToFailureHttpResult(result.ErrorMessage!);
    }

    public static async Task<IResult> GetBookingsForResourceAsync(
        Guid resourceId,
        GetBookingsForResourceHandler handler)
    {
        var query = new GetBookingsForResourceQuery(
            new ResourceId(resourceId));

        var bookings = await handler.HandleAsync(query);

        return Results.Ok(bookings);
    }

    public static async Task<IResult> GetCalendarForDayAsync(
        DateOnly date,
        GetCalendarForDayHandler handler)
    {
        var calendar = await handler.HandleAsync(
            new GetCalendarForDayQuery(date));

        return Results.Ok(calendar);
    }

    public static async Task<IResult> GetAvailableSlotsAsync(
        Guid resourceId,
        DateOnly date,
        GetAvailableSlotsHandler handler)
    {
        var query = new GetAvailableSlotsQuery(
            new ResourceId(resourceId),
            date);

        var slots = await handler.HandleAsync(query);

        return Results.Ok(slots);
    }

    private static IResult ToFailureHttpResult(string errorMessage)
    {
        var error = new ErrorResponse(errorMessage);

        return errorMessage switch
        {
            "Resource does not exist." => Results.NotFound(error),
            "Booking does not exist." => Results.NotFound(error),
            "Booking does not exist on this resource." =>
                Results.NotFound(error),
            "Resource is not available for this period." =>
                Results.Conflict(error),
            _ => Results.BadRequest(error)
        };
    }
}

public sealed record CreateBookingRequest(DateTime Start, DateTime End);

public sealed record BookingResponse(
    Guid Id,
    DateTime Start,
    DateTime End,
    string Status)
{
    public static BookingResponse From(Booking booking) =>
        new(
            booking.Id.Value,
            booking.Period.Start,
            booking.Period.End,
            booking.Status.ToString());
}

public sealed record ErrorResponse(string Error);
