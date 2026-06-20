using BookingDDD.Api;
using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Application;
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
builder.Services.AddScoped<BookingService>();

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

app.MapDelete(
    "/api/resources/{resourceId:guid}/bookings/{bookingId:guid}",
    BookingEndpoints.CancelBookingAsync);

app.Run();

public partial class Program;
