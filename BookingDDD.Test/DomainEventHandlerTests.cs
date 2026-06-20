using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Application;
using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

public class DomainEventHandlerTests
{
    [Test]
    public async Task BookingCreatedHandlers_RunAllApprovedConsequences()
    {
        var auditLog = new FakeAuditLog();
        var calendar = new FakeCalendar();
        var notification = new FakeNotification();
        var domainEvent = new BookingCreated(
            BookingId.New(),
            ResourceId.New(),
            new DateTime(2026, 6, 15, 10, 0, 0),
            new DateTime(2026, 6, 15, 11, 0, 0));

        await new AuditBookingCreatedHandler(auditLog)
            .HandleAsync(domainEvent);
        await new AddBookingToCalendarHandler(calendar)
            .HandleAsync(domainEvent);
        await new SendBookingConfirmationHandler(notification)
            .HandleAsync(domainEvent);

        Assert.Multiple(() =>
        {
            Assert.That(auditLog.EventNames, Is.EqualTo(
                new[] { nameof(BookingCreated) }));
            Assert.That(calendar.AddedBookingIds, Does.Contain(
                domainEvent.BookingId));
            Assert.That(notification.CreatedBookingIds, Does.Contain(
                domainEvent.BookingId));
        });
    }

    [Test]
    public async Task BookingCancelledHandlers_RunAllApprovedConsequences()
    {
        var auditLog = new FakeAuditLog();
        var calendar = new FakeCalendar();
        var notification = new FakeNotification();
        var domainEvent = new BookingCancelled(
            BookingId.New(),
            ResourceId.New(),
            new DateTime(2026, 6, 15, 10, 0, 0),
            new DateTime(2026, 6, 15, 11, 0, 0));

        await new AuditBookingCancelledHandler(auditLog)
            .HandleAsync(domainEvent);
        await new RemoveBookingFromCalendarHandler(calendar)
            .HandleAsync(domainEvent);
        await new SendBookingCancellationHandler(notification)
            .HandleAsync(domainEvent);

        Assert.Multiple(() =>
        {
            Assert.That(auditLog.EventNames, Is.EqualTo(
                new[] { nameof(BookingCancelled) }));
            Assert.That(calendar.RemovedBookingIds, Does.Contain(
                domainEvent.BookingId));
            Assert.That(notification.CancelledBookingIds, Does.Contain(
                domainEvent.BookingId));
        });
    }

    private sealed class FakeAuditLog : IAuditLog
    {
        public List<string> EventNames { get; } = [];

        public Task RecordAsync(
            string eventName,
            BookingId bookingId,
            ResourceId resourceId,
            DateTime occurredAtUtc)
        {
            EventNames.Add(eventName);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCalendar : IBookingCalendar
    {
        public List<BookingId> AddedBookingIds { get; } = [];
        public List<BookingId> RemovedBookingIds { get; } = [];

        public Task AddAsync(
            BookingId bookingId,
            ResourceId resourceId,
            DateTime start,
            DateTime end)
        {
            AddedBookingIds.Add(bookingId);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(BookingId bookingId)
        {
            RemovedBookingIds.Add(bookingId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotification : IBookingNotification
    {
        public List<BookingId> CreatedBookingIds { get; } = [];
        public List<BookingId> CancelledBookingIds { get; } = [];

        public Task SendCreatedAsync(
            BookingId bookingId,
            ResourceId resourceId,
            DateTime start,
            DateTime end)
        {
            CreatedBookingIds.Add(bookingId);
            return Task.CompletedTask;
        }

        public Task SendCancelledAsync(
            BookingId bookingId,
            ResourceId resourceId)
        {
            CancelledBookingIds.Add(bookingId);
            return Task.CompletedTask;
        }
    }
}
