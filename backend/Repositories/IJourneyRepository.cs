using Backend.Data;
using Backend.Models;

namespace Backend.Repositories;

public interface IJourneyRepository : IRepository<Journey>
{
    Task<List<Journey>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
}