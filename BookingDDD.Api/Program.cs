using BookingDDD.Api;
using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Application;
using BookingDDD.Core.Application.Commands.BookResource;
using BookingDDD.Core.Application.Commands.CancelBooking;
using BookingDDD.Core.Application.Queries;
using BookingDDD.Core.Application.Queries.GetAvailableSlots;
using BookingDDD.Core.Application.Queries.GetBookingsForResource;
using BookingDDD.Core.Application.Queries.GetCalendarForDay;
using BookingDDD.Core.Domain;
using BookingDDD.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookingDb")
    ?? throw new InvalidOperationException(
        "Connection string 'BookingDb' is missing.");

builder.Services.AddSingleton(new SqlServerOptions(connectionString));
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<DapperUnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<DapperUnitOfWork>());
builder.Services.AddScoped<IResourceRepository, DapperResourceRepository>();
builder.Services.AddScoped<BookResourceHandler>();
builder.Services.AddScoped<CancelBookingHandler>();

builder.Services.AddScoped<IBookingQueries, DapperBookingQueries>();
builder.Services.AddScoped<GetBookingsForResourceHandler>();
builder.Services.AddScoped<GetCalendarForDayHandler>();
builder.Services.AddScoped<GetAvailableSlotsHandler>();

// Reflection-based dispatcher. Replace DomainEventDispatcher with
// ManualDomainEventDispatcher to demonstrate explicit, hardcoded dispatch.
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

builder.Services.AddSingleton<IAuditLog, SqlAuditLog>();
builder.Services.AddSingleton<IBookingCalendar, SqlBookingCalendar>();
builder.Services.AddSingleton<IBookingNotification, ConsoleBookingNotification>();

builder.Services.AddTransient<AuditBookingCreatedHandler>();
builder.Services.AddTransient<AddBookingToCalendarHandler>();
builder.Services.AddTransient<SendBookingConfirmationHandler>();
builder.Services.AddTransient<AuditBookingCancelledHandler>();
builder.Services.AddTransient<RemoveBookingFromCalendarHandler>();
builder.Services.AddTransient<SendBookingCancellationHandler>();

builder.Services.AddTransient<
    IDomainEventHandler<BookingCreated>>(services =>
        services.GetRequiredService<AuditBookingCreatedHandler>());
builder.Services.AddTransient<
    IDomainEventHandler<BookingCreated>>(services =>
        services.GetRequiredService<AddBookingToCalendarHandler>());
builder.Services.AddTransient<
    IDomainEventHandler<BookingCreated>>(services =>
        services.GetRequiredService<SendBookingConfirmationHandler>());
builder.Services.AddTransient<
    IDomainEventHandler<BookingCancelled>>(services =>
        services.GetRequiredService<AuditBookingCancelledHandler>());
builder.Services.AddTransient<
    IDomainEventHandler<BookingCancelled>>(services =>
        services.GetRequiredService<RemoveBookingFromCalendarHandler>());
builder.Services.AddTransient<
    IDomainEventHandler<BookingCancelled>>(services =>
        services.GetRequiredService<SendBookingCancellationHandler>());

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost(
    "/api/resources/{resourceId:guid}/bookings",
    BookingEndpoints.CreateBookingAsync);

app.MapPost(
    "/api/bookings/{bookingId:guid}/cancel",
    BookingEndpoints.CancelBookingAsync);

app.MapGet(
    "/api/resources/{resourceId:guid}/bookings",
    BookingEndpoints.GetBookingsForResourceAsync);

app.MapGet(
    "/api/calendar",
    BookingEndpoints.GetCalendarForDayAsync);

app.MapGet(
    "/api/resources/{resourceId:guid}/available-slots",
    BookingEndpoints.GetAvailableSlotsAsync);

app.Run();

public partial class Program;
