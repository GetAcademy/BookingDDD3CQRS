using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Application;
using BookingDDD.Core.Domain;

namespace BookingDDD.Test;

public class BookingServiceTests
{
    [Test]
    public async Task BookAsync_SavesCommitsThenPublishes()
    {
        var resource = CreateResource();
        var repository = new FakeResourceRepository(resource);
        var unitOfWork = new FakeUnitOfWork();
        var dispatcher = new FakeDispatcher(unitOfWork);
        var service = new BookingService(
            repository,
            unitOfWork,
            dispatcher);

        var result = await service.BookAsync(
            resource.Id,
            TestPeriods.Create(10, 11));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(repository.SaveCount, Is.EqualTo(1));
            Assert.That(unitOfWork.CommitCount, Is.EqualTo(1));
            Assert.That(dispatcher.PublishCount, Is.EqualTo(1));
            Assert.That(dispatcher.WasCommittedWhenPublished, Is.True);
            Assert.That(dispatcher.Events.Single(),
                Is.TypeOf<BookingCreated>());
            Assert.That(resource.DomainEvents, Is.Empty);
        });
    }

    [Test]
    public async Task BookAsync_DoesNotPersistWhenDomainRejectsPeriod()
    {
        var resource = CreateResource();
        var repository = new FakeResourceRepository(resource);
        var unitOfWork = new FakeUnitOfWork();
        var dispatcher = new FakeDispatcher(unitOfWork);
        var service = new BookingService(
            repository,
            unitOfWork,
            dispatcher);

        var result = await service.BookAsync(
            resource.Id,
            TestPeriods.Create(18, 19));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(repository.SaveCount, Is.Zero);
            Assert.That(unitOfWork.CommitCount, Is.Zero);
            Assert.That(dispatcher.PublishCount, Is.Zero);
        });
    }

    [Test]
    public void BookAsync_RollsBackAndDoesNotPublishWhenCommitFails()
    {
        var resource = CreateResource();
        var repository = new FakeResourceRepository(resource);
        var unitOfWork = new FakeUnitOfWork { ThrowOnCommit = true };
        var dispatcher = new FakeDispatcher(unitOfWork);
        var service = new BookingService(
            repository,
            unitOfWork,
            dispatcher);

        Assert.That(
            async () => await service.BookAsync(
                resource.Id,
                TestPeriods.Create(10, 11)),
            Throws.TypeOf<InvalidOperationException>());

        Assert.Multiple(() =>
        {
            Assert.That(unitOfWork.RollbackCount, Is.EqualTo(1));
            Assert.That(dispatcher.PublishCount, Is.Zero);
        });
    }

    [Test]
    public async Task CancelAsync_PersistsCancellationAndPublishesEvent()
    {
        var resource = CreateResource();
        var booking = resource.Book(TestPeriods.Create(10, 11)).Value!;
        resource.ClearDomainEvents();
        var repository = new FakeResourceRepository(resource);
        var unitOfWork = new FakeUnitOfWork();
        var dispatcher = new FakeDispatcher(unitOfWork);
        var service = new BookingService(
            repository,
            unitOfWork,
            dispatcher);

        var result = await service.CancelAsync(
            resource.Id,
            booking.Id,
            new DateTime(2026, 6, 15, 9, 0, 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(repository.SaveCount, Is.EqualTo(1));
            Assert.That(unitOfWork.CommitCount, Is.EqualTo(1));
            Assert.That(dispatcher.Events.Single(),
                Is.TypeOf<BookingCancelled>());
        });
    }

    [Test]
    public async Task BookAsync_ReturnsFailureWhenResourceDoesNotExist()
    {
        var repository = new FakeResourceRepository(null);
        var unitOfWork = new FakeUnitOfWork();
        var dispatcher = new FakeDispatcher(unitOfWork);
        var service = new BookingService(
            repository,
            unitOfWork,
            dispatcher);

        var result = await service.BookAsync(
            ResourceId.New(),
            TestPeriods.Create(10, 11));

        Assert.That(result.ErrorMessage, Is.EqualTo(
            "Resource does not exist."));
        Assert.That(unitOfWork.CommitCount, Is.Zero);
    }

    private static Resource CreateResource() =>
        Resource.Create(
            ResourceId.New(),
            "Meeting room",
            new OpeningHours(8, 16));

    private sealed class FakeResourceRepository(Resource? resource)
        : IResourceRepository
    {
        public int SaveCount { get; private set; }

        public Task<Resource?> GetByIdAsync(ResourceId resourceId) =>
            Task.FromResult(resource);

        public Task SaveAsync(Resource aggregate)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public bool ThrowOnCommit { get; init; }
        public bool IsCommitted => CommitCount > 0 && !ThrowOnCommit;

        public Task CommitAsync()
        {
            CommitCount++;
            return ThrowOnCommit
                ? Task.FromException(new InvalidOperationException(
                    "Database commit failed."))
                : Task.CompletedTask;
        }

        public Task RollbackAsync()
        {
            RollbackCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDispatcher(FakeUnitOfWork unitOfWork)
        : IDomainEventDispatcher
    {
        public int PublishCount { get; private set; }
        public bool WasCommittedWhenPublished { get; private set; }
        public IReadOnlyCollection<IDomainEvent> Events { get; private set; } =
            Array.Empty<IDomainEvent>();

        public Task PublishAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents)
        {
            PublishCount++;
            WasCommittedWhenPublished = unitOfWork.IsCommitted;
            Events = domainEvents;
            return Task.CompletedTask;
        }
    }
}
