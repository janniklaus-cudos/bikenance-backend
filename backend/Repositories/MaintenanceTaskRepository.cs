using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public interface IMaintenanceTaskRepository : IRepository<MaintenanceTask>
{
    Task<List<MaintenanceTask>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
    Task<List<MaintenanceTask>> GetAllByBikePartIdAsync(Guid bikePartId, CancellationToken ct = default);
}

public class MaintenanceTaskRepository(AppDbContext db) : EfRepository<MaintenanceTask>(db), IMaintenanceTaskRepository
{
    public Task<List<MaintenanceTask>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default) =>
        _db.MaintenanceTasks.Where(mt => mt.BikePart.Bike.Id == bikeId).ToListAsync(ct);

    public Task<List<MaintenanceTask>> GetAllByBikePartIdAsync(Guid bikePartId, CancellationToken ct = default) =>
        _db.MaintenanceTasks.Where(mt => mt.BikePart.Id == bikePartId).ToListAsync(ct);
}