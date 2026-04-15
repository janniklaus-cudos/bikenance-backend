using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class BikePartRepository(AppDbContext db) : EfRepository<BikePart>(db), IBikePartRepository
{
    public Task<List<BikePart>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default) =>
        _db.BikeParts.Where(bp => bp.Bike.Id == bikeId).ToListAsync(ct);
}