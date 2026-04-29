using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public interface IBikeRepository : IRepository<Bike>
{
    // the two methods in the bike repo are to overwrite the default repo behavior, so here are no methods
}

public class BikeRepository(AppDbContext db, ICurrentUserService currentUserService) : EfRepository<Bike>(db), IBikeRepository
{
    public new Task<Bike?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Bikes
            .Where(b => b.Id == id)
            .Where(b => b.Owner.Id.ToString() == currentUserService.UserId)
            .FirstOrDefaultAsync(ct);

    public new Task<List<Bike>> GetAllAsync(CancellationToken ct = default) =>
        _db.Bikes
            .Where(b => b.Owner.Id.ToString() == currentUserService.UserId)
            .ToListAsync(ct);

}