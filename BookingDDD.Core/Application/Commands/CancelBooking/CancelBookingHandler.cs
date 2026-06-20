using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application.Commands.CancelBooking;

public sealed class CancelBookingHandler
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CancelBookingHandler(
        IResourceRepository resourceRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Booking>> HandleAsync(
        CancelBookingCommand command)
    {
        var resource =
            await _resourceRepository.GetByBookingIdAsync(command.BookingId);

        if (resource is null)
        {
            return Result<Booking>.Fail("Booking does not exist.");
        }

        var result = resource.CancelBooking(
            command.BookingId,
            command.Now);
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
