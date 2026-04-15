using Backend.Data;
using Backend.Models;

namespace Backend.Repositories;

public interface IMaintenanceTaskRepository : IRepository<MaintenanceTask>
{
    Task<List<MaintenanceTask>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
    Task<List<MaintenanceTask>> GetAllByBikePartIdAsync(Guid bikePartId, CancellationToken ct = default);
}