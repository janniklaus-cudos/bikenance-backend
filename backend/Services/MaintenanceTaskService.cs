using AutoMapper;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public interface IMaintenanceTaskService
{
    Task<List<MaintenanceTaskDto>?> GetAllByBikePartIdAsync(Guid bikePartId);
    Task<MaintenanceTaskDto?> AddAsync(Guid bikePartId, MaintenanceTaskDto maintenanceTask);
    Task<MaintenanceTaskDto?> UpdateAsync(Guid id, MaintenanceTaskDto maintenanceTask);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId);
}

public class MaintenanceTaskService(IMapper mapper, IMaintenanceTaskRepository maintenanceTaskRepository, IBikePartRepository bikePartRepository) : IMaintenanceTaskService
{
    public async Task<List<MaintenanceTaskDto>?> GetAllByBikePartIdAsync(Guid bikePartId)
    {
        var maintenanceTasks = await maintenanceTaskRepository.GetAllByBikePartIdAsync(bikePartId);
        if (maintenanceTasks == null)
        {
            return null;
        }

        return mapper.Map<List<MaintenanceTaskDto>>(maintenanceTasks);
    }

    public async Task<MaintenanceTaskDto?> AddAsync(Guid bikePartId, MaintenanceTaskDto maintenanceTask)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(bikePartId);
        if (bikePart == null)
        {
            return null;
        }

        var createdMaintenanceTask = new MaintenanceTask
        {
            BikePart = bikePart,
            Description = maintenanceTask.Description,
            IsDaysIntervalActive = maintenanceTask.IsDaysIntervalActive,
            DaysInterval = maintenanceTask.DaysInterval,
            IsDistanceIntervalActive = maintenanceTask.IsDistanceIntervalActive,
            DistanceInterval = maintenanceTask.DistanceInterval,
            Cost = maintenanceTask.Cost,
            Importance = maintenanceTask.Importance,
            IsActive = maintenanceTask.IsActive
        };

        maintenanceTaskRepository.Add(createdMaintenanceTask);
        await maintenanceTaskRepository.SaveChangesAsync();

        return mapper.Map<MaintenanceTaskDto>(createdMaintenanceTask);
    }

    public async Task<MaintenanceTaskDto?> UpdateAsync(Guid id, MaintenanceTaskDto maintenanceTask)
    {
        var existingTask = await maintenanceTaskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            return null;
        }

        existingTask.Description = maintenanceTask.Description;
        existingTask.IsDaysIntervalActive = maintenanceTask.IsDaysIntervalActive;
        existingTask.DaysInterval = maintenanceTask.DaysInterval;
        existingTask.IsDistanceIntervalActive = maintenanceTask.IsDistanceIntervalActive;
        existingTask.DistanceInterval = maintenanceTask.DistanceInterval;
        existingTask.Cost = maintenanceTask.Cost;
        existingTask.Importance = maintenanceTask.Importance;
        existingTask.IsActive = maintenanceTask.IsActive;

        maintenanceTaskRepository.Update(existingTask);
        await maintenanceTaskRepository.SaveChangesAsync();

        return mapper.Map<MaintenanceTaskDto>(existingTask);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await maintenanceTaskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return false;
        }

        maintenanceTaskRepository.Remove(task);
        await maintenanceTaskRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(bikePartId);
        if (bikePart == null)
        {
            return false;
        }

        var maintenanceTasks = await maintenanceTaskRepository.GetAllByBikePartIdAsync(bikePartId);
        if (maintenanceTasks.Count == 0)
        {
            return true;
        }

        maintenanceTaskRepository.RemoveRange(maintenanceTasks);
        await maintenanceTaskRepository.SaveChangesAsync();

        return true;
    }
}
