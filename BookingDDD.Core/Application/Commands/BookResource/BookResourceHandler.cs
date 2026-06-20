using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application.Commands.BookResource;

public sealed class BookResourceHandler
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public BookResourceHandler(
        IResourceRepository resourceRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Booking>> HandleAsync(
        BookResourceCommand command)
    {
        var periodResult = BookingPeriod.Create(
            command.Start,
            command.End);
        if (periodResult.IsFailure)
        {
            return Result<Booking>.Fail(periodResult.ErrorMessage!);
        }

        var resource =
            await _resourceRepository.GetByIdAsync(command.ResourceId);

        if (resource is null)
        {
            return Result<Booking>.Fail("Resource does not exist.");
        }

        var result = resource.Book(periodResult.Value!);
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
