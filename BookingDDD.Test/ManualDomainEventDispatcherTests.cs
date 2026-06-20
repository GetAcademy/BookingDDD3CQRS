using BookingDDD.Api;
using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Application;
using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

public class ManualDomainEventDispatcherTests
{
    [Test]
    public async Task PublishAsync_CallsAllHardcodedHandlers()
    {
        var auditLog = new FakeAuditLog();
        var calendar = new FakeCalendar();
        var notification = new FakeNotification();
        var dispatcher = CreateDispatcher(
            auditLog,
            calendar,
            notification);
        var resourceId = ResourceId.New();
        var created = new BookingCreated(
            BookingId.New(),
            resourceId,
            new DateTime(2026, 6, 15, 10, 0, 0),
            new DateTime(2026, 6, 15, 11, 0, 0));
        var cancelled = new BookingCancelled(
            created.BookingId,
            resourceId,
            created.Start,
            created.End);

        await dispatcher.PublishAsync([created, cancelled]);

        Assert.Multiple(() =>
        {
            Assert.That(auditLog.EventNames, Is.EqualTo(
                new[]
                {
                    nameof(BookingCreated),
                    nameof(BookingCancelled)
                }));
            Assert.That(calendar.AddedBookingIds, Does.Contain(
                created.BookingId));
            Assert.That(calendar.RemovedBookingIds, Does.Contain(
                cancelled.BookingId));
            Assert.That(notification.CreatedBookingIds, Does.Contain(
                created.BookingId));
            Assert.That(notification.CancelledBookingIds, Does.Contain(
                cancelled.BookingId));
        });
    }

    [Test]
    public void PublishAsync_ThrowsForEventThatWasNotAddedManually()
    {
        var dispatcher = CreateDispatcher(
            new FakeAuditLog(),
            new FakeCalendar(),
            new FakeNotification());

        Assert.That(
            async () => await dispatcher.PublishAsync([new UnknownEvent()]),
            Throws.TypeOf<NotSupportedException>()
                .With.Message.Contains(nameof(UnknownEvent)));
    }

    private static ManualDomainEventDispatcher CreateDispatcher(
        IAuditLog auditLog,
        IBookingCalendar calendar,
        IBookingNotification notification) =>
        new(
            new AuditBookingCreatedHandler(auditLog),
            new AddBookingToCalendarHandler(calendar),
            new SendBookingConfirmationHandler(notification),
            new AuditBookingCancelledHandler(auditLog),
            new RemoveBookingFromCalendarHandler(calendar),
            new SendBookingCancellationHandler(notification));

    private sealed record UnknownEvent : IDomainEvent;

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
