using BookingDDD.Core.Application;
using BookingDDD.Core.Domain;

namespace BookingDDD.Api;

public static class BookingEndpoints
{
    public static async Task<IResult> CreateBookingAsync(
        Guid resourceId,
        CreateBookingRequest request,
        BookingService bookingService)
    {
        var periodResult = BookingPeriod.Create(request.Start, request.End);
        if (periodResult.IsFailure)
        {
            return Results.BadRequest(new ErrorResponse(
                periodResult.ErrorMessage!));
        }

        var result = await bookingService.BookAsync(
            new ResourceId(resourceId),
            periodResult.Value!);

        return ToHttpResult(result);
    }

    public static async Task<IResult> CancelBookingAsync(
        Guid resourceId,
        Guid bookingId,
        BookingService bookingService,
        TimeProvider timeProvider)
    {
        var result = await bookingService.CancelAsync(
            new ResourceId(resourceId),
            new BookingId(bookingId),
            timeProvider.GetLocalNow().DateTime);

        return ToHttpResult(result);
    }

    private static IResult ToHttpResult(Result<Booking> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(BookingResponse.From(result.Value!));
        }

        var error = new ErrorResponse(result.ErrorMessage!);
        return result.ErrorMessage == "Resource does not exist."
            ? Results.NotFound(error)
            : Results.BadRequest(error);
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
