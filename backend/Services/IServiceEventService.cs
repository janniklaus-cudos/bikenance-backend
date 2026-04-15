using Backend.Dtos;

namespace Backend.Services;

public interface IServiceEventService
{
    Task<ServiceEventDto?> GetByIdAsync(Guid id);
    Task<List<ServiceEventDto>?> GetAllByBikePartIdAsync(Guid bikePartId);
    Task<List<ServiceEventDto>?> GetAllByBikeIdAsync(Guid bikeId);
    Task<ServiceEventDto?> AddAsync(Guid bikePartId, ServiceEventDto serviceEvent);
    Task<ServiceEventDto?> UpdateAsync(Guid id, ServiceEventDto serviceEvent);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId);
    Task<bool> DeleteAllByBikeIdAsync(Guid bikeId);
}
