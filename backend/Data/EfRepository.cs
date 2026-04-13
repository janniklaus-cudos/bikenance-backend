using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class EfRepository<T>(AppDbContext db) : IRepository<T> where T : class
{
    public IQueryable<T> Query() => db.Set<T>();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Set<T>().FindAsync([id], ct).AsTask();

    public Task<List<T>> GetAllAsync(CancellationToken ct = default) =>
        db.Set<T>().ToListAsync(ct);

    public void Add(T entity) =>
        db.Set<T>().Add(entity);

    public void AddRange(IEnumerable<T> entities) =>
        db.Set<T>().AddRange(entities);

    public void Update(T entity) =>
        db.Set<T>().Update(entity);
    public void UpdateRange(IEnumerable<T> entities) =>
        db.Set<T>().UpdateRange(entities);

    public void Remove(T entity) =>
        db.Set<T>().Remove(entity);
    public void RemoveRange(IEnumerable<T> entities) =>
        db.Set<T>().RemoveRange(entities);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}