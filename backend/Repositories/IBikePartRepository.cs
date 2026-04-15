using Backend.Data;
using Backend.Models;

namespace Backend.Repositories;

public interface IBikePartRepository : IRepository<BikePart>
{
    Task<List<BikePart>> GetAllByBikeIdAsync(Guid bikeId, CancellationToken ct = default);
}