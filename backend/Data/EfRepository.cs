using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class EfRepository<T>(AppDbContext db) : IRepository<T> where T : class
{
    protected readonly AppDbContext _db = db;

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Set<T>().FindAsync([id], ct).AsTask();

    public Task<List<T>> GetAllAsync(CancellationToken ct = default) =>
        _db.Set<T>().ToListAsync(ct);

    public void Add(T entity) =>
        _db.Set<T>().Add(entity);

    public void AddRange(IEnumerable<T> entities) =>
        _db.Set<T>().AddRange(entities);

    public void Update(T entity) =>
        _db.Set<T>().Update(entity);
    public void UpdateRange(IEnumerable<T> entities) =>
        _db.Set<T>().UpdateRange(entities);

    public void Remove(T entity) =>
        _db.Set<T>().Remove(entity);
    public void RemoveRange(IEnumerable<T> entities) =>
        _db.Set<T>().RemoveRange(entities);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}