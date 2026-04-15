using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class MaintenanceTaskRepository(AppDbContext db) : EfRepository<MaintenanceTask>(db), IMaintenanceTaskRepository
{
    public Task<List<MaintenanceTask>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default) =>
        db.MaintenanceTasks.Where(mt => mt.BikePart.Bike.Id == bikeId).ToListAsync(ct);

    public Task<List<MaintenanceTask>> GetAllByBikePartIdAsync(Guid bikePartId, CancellationToken ct = default) =>
        db.MaintenanceTasks.Where(mt => mt.BikePart.Id == bikePartId).ToListAsync(ct);
}