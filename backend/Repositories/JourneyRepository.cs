using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class JourneyRepository(AppDbContext db) : EfRepository<Journey>(db), IJourneyRepository
{
    public Task<List<Journey>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default) =>
        _db.Journeys.Where(journey => journey.Bike.Id == bikeId).ToListAsync(ct);
}