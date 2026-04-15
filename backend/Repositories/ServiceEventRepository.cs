using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ServiceEventRepository(AppDbContext db) : EfRepository<ServiceEvent>(db), IServiceEventRepository
{
    public Task<List<ServiceEvent>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default) =>
        _db.ServiceEvents.Where(se => se.BikePart.Bike.Id == bikeId).ToListAsync(ct);

    public Task<List<ServiceEvent>> GetAllByBikePartIdAsync(Guid bikePartId, CancellationToken ct = default) =>
        _db.ServiceEvents.Where(se => se.BikePart.Id == bikePartId).ToListAsync(ct);
}