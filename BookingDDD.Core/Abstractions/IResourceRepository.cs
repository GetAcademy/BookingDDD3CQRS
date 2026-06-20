using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(ResourceId resourceId);

    Task SaveAsync(Resource resource);
}
