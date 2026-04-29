using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Backend.Data;

public interface IRepository<T> where T : class
{
    DatabaseFacade GetDatabase();
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);

    void Add(T entity);
    void AddRange(IEnumerable<T> entities);

    void Update(T entity) => Update(entity);
    void UpdateRange(IEnumerable<T> entities);

    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    Task SaveChangesAsync(CancellationToken ct = default);
}