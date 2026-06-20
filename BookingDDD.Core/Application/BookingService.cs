using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application;

public sealed class BookingService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public BookingService(
        IResourceRepository resourceRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Booking>> BookAsync(
        ResourceId resourceId,
        BookingPeriod period)
    {
        var resource = await _resourceRepository.GetByIdAsync(resourceId);

        if (resource is null)
        {
            return Result<Booking>.Fail("Resource does not exist.");
        }

        var result = resource.Book(period);
        if (result.IsFailure)
        {
            return result;
        }

        await SaveCommitAndPublishAsync(resource);
        return result;
    }

    public async Task<Result<Booking>> CancelAsync(
        ResourceId resourceId,
        BookingId bookingId,
        DateTime now)
    {
        var resource = await _resourceRepository.GetByIdAsync(resourceId);

        if (resource is null)
        {
            return Result<Booking>.Fail("Resource does not exist.");
        }

        var result = resource.CancelBooking(bookingId, now);
        if (result.IsFailure)
        {
            return result;
        }

        await SaveCommitAndPublishAsync(resource);
        return result;
    }

    private async Task SaveCommitAndPublishAsync(Resource resource)
    {
        try
        {
            await _resourceRepository.SaveAsync(resource);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        var events = resource.DomainEvents.ToArray();
        await _eventDispatcher.PublishAsync(events);
        resource.ClearDomainEvents();
    }
}
