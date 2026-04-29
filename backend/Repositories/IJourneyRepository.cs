using Backend.Data;
using Backend.Models;

namespace Backend.Repositories;

public interface IJourneyRepository : IRepository<Journey>
{
    Task<List<Journey>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
    Task<int> GetDistanceAfterDateByBikeId(Guid bikeId, DateOnly date, CancellationToken ct = default);
    Task<int> GetDistanceBeforeDateByBikeId(Guid bikeId, DateOnly date, CancellationToken ct = default);
}