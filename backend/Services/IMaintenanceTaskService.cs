using Backend.Dtos;
using Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services;

public interface IMaintenanceTaskService
{
    Task<List<MaintenanceTaskDto>?> GetAllByBikePartIdAsync(Guid bikePartId);
    Task<MaintenanceTaskDto?> AddAsync(Guid bikePartId, MaintenanceTaskDto maintenanceTask);
    Task<MaintenanceTaskDto?> UpdateAsync(Guid id, MaintenanceTaskDto maintenanceTask);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId);
}
