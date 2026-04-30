using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public interface IJourneyRepository : IRepository<Journey>
{
    Task<List<Journey>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
    Task<List<Journey>> GetAllByDate(DateOnly date, CancellationToken ct = default);
    Task<int> GetDistanceAfterDateByBikeId(Guid bikeId, DateOnly date, CancellationToken ct = default);
    Task<int> GetDistanceBeforeDateByBikeId(Guid bikeId, DateOnly date, CancellationToken ct = default);

    Task<List<Journey>> GetAllConnectedToRepeatedJourney(RepeatJourney repeatJourney, bool isConnected, CancellationToken ct = default);
}

public class JourneyRepository(AppDbContext db) : EfRepository<Journey>(db), IJourneyRepository
{
    public Task<List<Journey>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default) =>
        _db.Journeys.Where(journey => journey.Bike.Id == bikeId)
                    .ToListAsync(ct);

    public Task<List<Journey>> GetAllByDate(DateOnly date, CancellationToken ct = default) =>
        _db.Journeys.Where(journey => journey.JourneyDate == date)
                    .ToListAsync(ct);

    public Task<int> GetDistanceAfterDateByBikeId(Guid bikeId, DateOnly date, CancellationToken ct = default) =>
        _db.Journeys.Where(journey => journey.Bike.Id == bikeId && journey.JourneyDate.CompareTo(date) >= 0)
                    .SumAsync(journey => journey.Distance, ct);

    public Task<int> GetDistanceBeforeDateByBikeId(Guid bikeId, DateOnly date, CancellationToken ct = default) =>
        _db.Journeys.Where(journey => journey.Bike.Id == bikeId && journey.JourneyDate.CompareTo(date) <= 0)
                    .SumAsync(journey => journey.Distance, ct);

    public Task<List<Journey>> GetAllConnectedToRepeatedJourney(RepeatJourney repeatJourney, bool isConnected, CancellationToken ct = default) =>
        _db.Journeys.Where(journey => journey.IsConnectedToRepeatJourney == isConnected && journey.RepeatJourney != null && journey.RepeatJourney.Id == repeatJourney.Id)
                    .ToListAsync(ct);

}