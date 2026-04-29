using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public interface IRepeatJourneyRepository : IRepository<RepeatJourney>
{
    Task<List<RepeatJourney>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
}

public class RepeatJourneyRepository(AppDbContext db) : EfRepository<RepeatJourney>(db), IRepeatJourneyRepository
{
    public Task<List<RepeatJourney>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default) =>
        _db.RepeatJourneys.Where(repeatJourney => repeatJourney.Bike.Id == bikeId)
                    .ToListAsync(ct);
}