using Backend.Data;
using Backend.Models;

namespace Backend.Repositories;

public interface IServiceEventRepository : IRepository<ServiceEvent>
{
    Task<List<ServiceEvent>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
    Task<List<ServiceEvent>> GetAllByBikePartIdAsync(Guid bikePartId, CancellationToken ct = default);
}