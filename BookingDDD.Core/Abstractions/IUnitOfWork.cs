namespace BookingDDD.Core.Abstractions;

public interface IUnitOfWork
{
    Task CommitAsync();

    Task RollbackAsync();
}
